namespace StarCorp.FlightBooking.Core.Models;

public class Airline
{
    public int    Id       { get; set; }
    public string Name     { get; set; } = string.Empty;
    public string IataCode { get; set; } = string.Empty;
    public string Country  { get; set; } = string.Empty;
}
