using StarCorp.FlightBooking.Core.Enums;

namespace StarCorp.FlightBooking.Core.Models;

public class Payment
{
    public int Id { get; set; }
    public int BookingId { get; set; }
    public PaymentMethod Method { get; set; }
    public decimal Amount { get; set; }
    public string Status { get; set; } = "Pending";
    public DateTime? PaidAt { get; set; }
    public string? TransactionId { get; set; }
}
