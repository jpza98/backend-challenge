using StarCorp.FlightBooking.Core.Enums;

namespace StarCorp.FlightBooking.Core.Models;

public class Booking
{
    public int Id { get; set; }
    public string BookingCode { get; set; } = string.Empty;
    public int CustomerId { get; set; }
    public Customer? Customer { get; set; }
    public int FlightId { get; set; }
    public Flight? Flight { get; set; }
    public FareClass FareClass { get; set; }
    public BookingStatus Status { get; set; }
    public decimal TotalAmount { get; set; }
    public DateTime CreatedAt { get; set; }
    public List<Passenger> Passengers { get; set; } = [];
    public Payment? Payment { get; set; }
}
