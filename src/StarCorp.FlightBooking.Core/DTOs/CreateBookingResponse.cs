using StarCorp.FlightBooking.Core.Models;

namespace StarCorp.FlightBooking.Core.DTOs;

public class CreateBookingResponse
{
    public Booking Booking { get; init; } = null!;
    public PriceCalculationResult PriceBreakdown { get; init; } = null!;
}
