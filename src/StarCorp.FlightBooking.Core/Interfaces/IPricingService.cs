using StarCorp.FlightBooking.Core.DTOs;
using StarCorp.FlightBooking.Core.Enums;

namespace StarCorp.FlightBooking.Core.Interfaces;

public interface IPricingService
{
    PriceCalculationResult CalculatePrice(
        decimal       basePriceEconomy,
        FareClass     fareClass,
        int           passengerCount,
        PaymentMethod paymentMethod);
}
