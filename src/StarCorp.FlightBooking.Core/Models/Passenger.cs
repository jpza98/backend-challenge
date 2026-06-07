namespace StarCorp.FlightBooking.Core.Models;

public class Passenger
{
    public int Id { get; set; }
    public int BookingId { get; set; }
    public string Name { get; set; } = string.Empty;
    public string CPF { get; set; } = string.Empty;
    public DateTime BirthDate { get; set; }
    public string? SeatNumber { get; set; }
}
