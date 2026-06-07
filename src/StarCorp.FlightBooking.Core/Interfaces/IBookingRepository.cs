using StarCorp.FlightBooking.Core.Enums;
using StarCorp.FlightBooking.Core.Models;

namespace StarCorp.FlightBooking.Core.Interfaces;

public interface IBookingRepository
{
    Task<Booking?> GetByIdAsync(int id);
    Task<Booking?> GetByCodeAsync(string code);
    Task<IEnumerable<Booking>> GetByCustomerIdAsync(int customerId);
    Task<Booking> CreateAsync(Booking booking);
    Task<bool> UpdateStatusAsync(int id, BookingStatus status);
    Task AddPaymentAsync(Payment payment);
    Task<bool> UpdateTotalAmountAsync(int bookingId, decimal totalAmount);
}
