using StarCorp.FlightBooking.Core.Enums;

namespace StarCorp.FlightBooking.Core.Models;

public class Flight
{
    public int Id { get; set; }
    public string FlightNumber { get; set; } = string.Empty;
    public int AirlineId { get; set; }
    public Airline? Airline { get; set; }
    public string Origin { get; set; } = string.Empty;
    public string Destination { get; set; } = string.Empty;
    public DateTime DepartureTime { get; set; }
    public DateTime ArrivalTime { get; set; }
    public decimal BasePriceEconomy { get; set; }
    public int AvailableSeatsEconomy { get; set; }
    public int AvailableSeatsExecutive { get; set; }
    public string Status { get; set; } = "Scheduled";

    public TimeSpan Duration => ArrivalTime - DepartureTime;

    public bool HasAvailableSeats(FareClass fareClass, int count) =>
        fareClass == FareClass.Economy
            ? AvailableSeatsEconomy >= count
            : AvailableSeatsExecutive >= count;
}
