using StarCorp.FlightBooking.Core.Enums;
using StarCorp.FlightBooking.Core.Models;

namespace StarCorp.FlightBooking.Core.Interfaces;

public interface IFlightRepository
{
    Task<Flight?> GetByIdAsync(int id);
    Task<IEnumerable<Flight>> SearchAsync(string? origin, string? destination, DateTime? date, FareClass? fareClass, int passengers, decimal? minPrice = null, decimal? maxPrice = null);
    Task<bool> UpdateSeatsAsync(int flightId, FareClass fareClass, int delta);
}
