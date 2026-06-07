using Microsoft.AspNetCore.Mvc;
using StarCorp.FlightBooking.Core.Enums;
using StarCorp.FlightBooking.Core.Interfaces;

namespace StarCorp.FlightBooking.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class FlightsController : ControllerBase
{
    private readonly IFlightRepository _flightRepo;
<<<<<<< HEAD
    private readonly IPricingService _pricingService;

    public FlightsController(IFlightRepository flightRepo, IPricingService pricingService)
    {
        _flightRepo = flightRepo;
        _pricingService = pricingService;
    }

    /// <summary>Pesquisa voos disponíveis.</summary>
    [HttpGet]
    public async Task<IActionResult> Search(
        [FromQuery] string? origin,
        [FromQuery] string? destination,
        [FromQuery] DateTime? date,
        [FromQuery] FareClass? fareClass,
        [FromQuery] int passengers = 1)
    {
        if (passengers < 1)
            return BadRequest("O número de passageiros deve ser pelo menos 1.");

        var flights = await _flightRepo.SearchAsync(origin, destination, date, fareClass, passengers);
        return Ok(flights);
    }

    /// <summary>Retorna um voo pelo ID.</summary>
    [HttpGet("{id:int}")]
    public async Task<IActionResult> GetById(int id)
    {
        var flight = await _flightRepo.GetByIdAsync(id);
        return flight is null ? NotFound() : Ok(flight);
    }

    /// <summary>Simula o preço de um voo para os parâmetros fornecidos.</summary>
    [HttpGet("{id:int}/price")]
    public async Task<IActionResult> GetPrice(
        int id,
        [FromQuery] FareClass fareClass,
        [FromQuery] int passengers,
        [FromQuery] PaymentMethod paymentMethod)
    {
        if (passengers < 1)
            return BadRequest("O número de passageiros deve ser pelo menos 1.");

        var flight = await _flightRepo.GetByIdAsync(id);
        if (flight is null) return NotFound();

        if (!flight.HasAvailableSeats(fareClass, passengers))
            return Conflict("Assentos insuficientes para a classe e quantidade solicitada.");

        var result = _pricingService.CalculatePrice(
            flight.BasePriceEconomy, fareClass, passengers, paymentMethod);

        return Ok(result);
    }
}
=======
        private readonly IPricingService   _pricingService;

            public FlightsController(IFlightRepository flightRepo, IPricingService pricingService)
                {
                        _flightRepo     = flightRepo;
                                _pricingService = pricingService;
                                    }

                                        /// <summary>Pesquisa voos disponíveis com filtros e paginação.</summary>
                                            [HttpGet]
                                                public async Task<IActionResult> Search(
                                                        [FromQuery] string?    origin,
                                                                [FromQuery] string?    destination,
                                                                        [FromQuery] DateTime?  date,
                                                                                [FromQuery] FareClass? fareClass,
                                                                                        [FromQuery] int        passengers  = 1,
                                                                                                [FromQuery] decimal?   minPrice    = null,
                                                                                                        [FromQuery] decimal?   maxPrice    = null,
                                                                                                                [FromQuery] int        page        = 1,
                                                                                                                        [FromQuery] int        pageSize    = 10)
                                                                                                                            {
                                                                                                                                    if (passengers < 1)
                                                                                                                                                return BadRequest("O número de passageiros deve ser pelo menos 1.");
                                                                                                                                                
                                                                                                                                                        if (page < 1)    page     = 1;
                                                                                                                                                                if (pageSize < 1 || pageSize > 100) pageSize = 10;
                                                                                                                                                                
                                                                                                                                                                        var flights = await _flightRepo.SearchAsync(origin, destination, date, fareClass, passengers, minPrice, maxPrice);
                                                                                                                                                                        
                                                                                                                                                                                var total      = flights.Count();
                                                                                                                                                                                        var totalPages = (int)Math.Ceiling(total / (double)pageSize);
                                                                                                                                                                                                var items      = flights.Skip((page - 1) * pageSize).Take(pageSize);
                                                                                                                                                                                                
                                                                                                                                                                                                        return Ok(new
                                                                                                                                                                                                                {
                                                                                                                                                                                                                            Page       = page,
                                                                                                                                                                                                                                        PageSize   = pageSize,
                                                                                                                                                                                                                                                    Total      = total,
                                                                                                                                                                                                                                                                TotalPages = totalPages,
                                                                                                                                                                                                                                                                            Items      = items
                                                                                                                                                                                                                                                                                    });
                                                                                                                                                                                                                                                                                        }
                                                                                                                                                                                                                                                                                        
                                                                                                                                                                                                                                                                                            /// <summary>Retorna um voo pelo ID.</summary>
                                                                                                                                                                                                                                                                                                [HttpGet("{id:int}")]
                                                                                                                                                                                                                                                                                                    public async Task<IActionResult> GetById(int id)
                                                                                                                                                                                                                                                                                                        {
                                                                                                                                                                                                                                                                                                                var flight = await _flightRepo.GetByIdAsync(id);
                                                                                                                                                                                                                                                                                                                        return flight == null ? NotFound() : Ok(flight);
                                                                                                                                                                                                                                                                                                                            }
                                                                                                                                                                                                                                                                                                                            
                                                                                                                                                                                                                                                                                                                                /// <summary>Simula o preço de um voo para os parâmetros fornecidos.</summary>
                                                                                                                                                                                                                                                                                                                                    [HttpGet("{id:int}/price")]
                                                                                                                                                                                                                                                                                                                                        public async Task<IActionResult> GetPrice(
                                                                                                                                                                                                                                                                                                                                                int id,
                                                                                                                                                                                                                                                                                                                                                        [FromQuery] FareClass     fareClass,
                                                                                                                                                                                                                                                                                                                                                                [FromQuery] int           passengers,
                                                                                                                                                                                                                                                                                                                                                                        [FromQuery] PaymentMethod paymentMethod)
                                                                                                                                                                                                                                                                                                                                                                            {
                                                                                                                                                                                                                                                                                                                                                                                    if (passengers < 1)
                                                                                                                                                                                                                                                                                                                                                                                                return BadRequest("O número de passageiros deve ser pelo menos 1.");
                                                                                                                                                                                                                                                                                                                                                                                                
                                                                                                                                                                                                                                                                                                                                                                                                        var flight = await _flightRepo.GetByIdAsync(id);
                                                                                                                                                                                                                                                                                                                                                                                                                if (flight == null) return NotFound();
                                                                                                                                                                                                                                                                                                                                                                                                                
                                                                                                                                                                                                                                                                                                                                                                                                                        if (!flight.HasAvailableSeats(fareClass, passengers))
                                                                                                                                                                                                                                                                                                                                                                                                                                    return Conflict("Assentos insuficientes para a classe e quantidade solicitada.");
                                                                                                                                                                                                                                                                                                                                                                                                                                    
                                                                                                                                                                                                                                                                                                                                                                                                                                            var result = _pricingService.CalculatePrice(flight.BasePriceEconomy, fareClass, passengers, paymentMethod);
                                                                                                                                                                                                                                                                                                                                                                                                                                                    return Ok(result);
                                                                                                                                                                                                                                                                                                                                                                                                                                                        }
                                                                                                                                                                                                                                                                                                                                                                                                                                                        }
>>>>>>> 9cff74b0823685c7d7cc1aa676ab89b47311490f
