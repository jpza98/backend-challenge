using StarCorp.FlightBooking.Core.DTOs;
using StarCorp.FlightBooking.Core.Enums;

namespace StarCorp.FlightBooking.Core.Interfaces;

public interface IPricingService
{
    /// <summary>Subtotal sem ajuste de forma de pagamento (base + impostos + taxa de serviço).</summary>
    PriceCalculationResult CalculatePrePaymentPrice(
        decimal basePriceEconomy,
        FareClass fareClass,
        int passengerCount);

    /// <summary>Total final com ajuste de forma de pagamento aplicado sobre o subtotal.</summary>
    PriceCalculationResult CalculatePrice(
        decimal basePriceEconomy,
        FareClass fareClass,
        int passengerCount,
        PaymentMethod paymentMethod);
}
