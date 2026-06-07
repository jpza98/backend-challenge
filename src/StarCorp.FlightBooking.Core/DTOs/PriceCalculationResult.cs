namespace StarCorp.FlightBooking.Core.DTOs;

public class PriceCalculationResult
{
    public decimal BaseAmount { get; init; }
    public decimal Taxes { get; init; }
    public decimal ServiceFee { get; init; }
    public decimal PaymentAdjustment { get; init; }
    public decimal TotalAmount { get; init; }
}
