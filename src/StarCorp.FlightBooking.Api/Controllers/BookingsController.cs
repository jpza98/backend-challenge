using Microsoft.AspNetCore.Mvc;
using StarCorp.FlightBooking.Core.DTOs;
using StarCorp.FlightBooking.Core.Enums;
using StarCorp.FlightBooking.Core.Interfaces;
using StarCorp.FlightBooking.Core.Models;

namespace StarCorp.FlightBooking.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class BookingsController : ControllerBase
{
    private readonly IBookingRepository _bookingRepo;
    private readonly IFlightRepository _flightRepo;
    private readonly ICustomerRepository _customerRepo;
    private readonly IPricingService _pricingService;
    private readonly ICancellationService _cancellationService;

    public BookingsController(
        IBookingRepository bookingRepo,
        IFlightRepository flightRepo,
        ICustomerRepository customerRepo,
        IPricingService pricingService,
        ICancellationService cancellationService)
    {
        _bookingRepo = bookingRepo;
        _flightRepo = flightRepo;
        _customerRepo = customerRepo;
        _pricingService = pricingService;
        _cancellationService = cancellationService;
    }

    /// <summary>Cria uma nova reserva.</summary>
    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateBookingRequest request)
    {
        var flight = await _flightRepo.GetByIdAsync(request.FlightId);
        if (flight is null)
            return NotFound("Voo não encontrado.");

        if (flight.Status != "Scheduled")
            return Conflict("Voo não está disponível para reserva.");

        if (!flight.HasAvailableSeats(request.FareClass, request.Passengers.Count))
            return Conflict("Assentos insuficientes para a classe e quantidade de passageiros solicitada.");

        var existingCustomer = await _customerRepo.GetByCPFAsync(request.Customer.CPF);
        if (existingCustomer is not null && !existingCustomer.IsActive)
            return UnprocessableEntity(new { reason = "Cliente inativo. Não é possível realizar reservas." });

        var customer = existingCustomer
                    ?? await _customerRepo.CreateAsync(new Customer
                    {
                        Name = request.Customer.Name,
                        Email = request.Customer.Email,
                        CPF = request.Customer.CPF,
                        Phone = request.Customer.Phone
                    });

        // Subtotal sem ajuste de pagamento — persistido em TotalAmount
        var prePayment = _pricingService.CalculatePrePaymentPrice(
            flight.BasePriceEconomy,
            request.FareClass,
            request.Passengers.Count);

        // Total final com ajuste de pagamento — cobrado no Payment
        var finalPrice = _pricingService.CalculatePrice(
            flight.BasePriceEconomy,
            request.FareClass,
            request.Passengers.Count,
            request.PaymentMethod);

        var booking = new Booking
        {
            BookingCode = GenerateBookingCode(),
            CustomerId = customer.Id,
            FlightId = request.FlightId,
            FareClass = request.FareClass,
            Status = BookingStatus.Confirmed,
            TotalAmount = prePayment.TotalAmount,
            Passengers = request.Passengers
                .Select(p => new Passenger
                {
                    Name = p.Name,
                    CPF = p.CPF,
                    BirthDate = p.BirthDate
                }).ToList(),
            Payment = new Payment
            {
                Method = request.PaymentMethod,
                Amount = finalPrice.TotalAmount,
                Status = "Confirmed",
                PaidAt = DateTime.UtcNow,
                TransactionId = Guid.NewGuid().ToString("N")[..16].ToUpper()
            }
        };

        var created = await _bookingRepo.CreateAsync(booking);
        await _flightRepo.UpdateSeatsAsync(request.FlightId, request.FareClass, -request.Passengers.Count);

        return CreatedAtAction(nameof(GetById), new { id = created.Id }, new CreateBookingResponse
        {
            Booking = created,
            PriceBreakdown = prePayment
        });
    }

    /// <summary>Retorna uma reserva pelo ID.</summary>
    [HttpGet("{id:int}")]
    public async Task<IActionResult> GetById(int id)
    {
        var booking = await _bookingRepo.GetByIdAsync(id);
        return booking is null ? NotFound() : Ok(booking);
    }

    /// <summary>Retorna uma reserva pelo código de reserva.</summary>
    [HttpGet("code/{code}")]
    public async Task<IActionResult> GetByCode(string code)
    {
        var booking = await _bookingRepo.GetByCodeAsync(code);
        return booking is null ? NotFound() : Ok(booking);
    }

    /// <summary>Lista todas as reservas de um cliente.</summary>
    [HttpGet("customer/{customerId:int}")]
    public async Task<IActionResult> GetByCustomer(int customerId)
    {
        var bookings = await _bookingRepo.GetByCustomerIdAsync(customerId);
        return Ok(bookings);
    }

    /// <summary>Cancela uma reserva e calcula o reembolso.</summary>
    [HttpPost("{id:int}/cancel")]
    public async Task<IActionResult> Cancel(int id)
    {
        var booking = await _bookingRepo.GetByIdAsync(id);
        if (booking is null) return NotFound();

        var refund = _cancellationService.CalculateRefund(booking, DateTime.UtcNow);
        if (!refund.IsSuccess)
            return UnprocessableEntity(new { refund.Reason });

        await _bookingRepo.UpdateStatusAsync(id, BookingStatus.Cancelled);

        // Devolve assentos ao voo
        await _flightRepo.UpdateSeatsAsync(booking.FlightId, booking.FareClass, booking.Passengers.Count);

        return Ok(new
        {
            BookingId = id,
            BookingCode = booking.BookingCode,
            refund.RefundAmount,
            refund.RefundPercentage,
            refund.Reason
        });
    }

    private static string GenerateBookingCode() =>
        Guid.NewGuid().ToString("N")[..8].ToUpper();
}
