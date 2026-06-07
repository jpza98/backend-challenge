using StarCorp.FlightBooking.Core.DTOs;
using StarCorp.FlightBooking.Core.Enums;
using StarCorp.FlightBooking.Core.Interfaces;

namespace StarCorp.FlightBooking.Core.Services;

public class PricingService : IPricingService
{
    private const decimal EconomyMultiplier = 1.0m;
    private const decimal ExecutiveMultiplier = 2.5m;
    private const decimal TaxRate = 0.08m;
    private const decimal TaxPerPassenger = 45m;
    private const decimal ServiceFeeRate = 0.05m;
    private const decimal CreditCardRate = 0.03m;
    private const decimal PixRate = -0.05m;
    private const decimal BoletoRate = 0.01m;

    public PriceCalculationResult CalculatePrePaymentPrice(
        decimal basePriceEconomy,
        FareClass fareClass,
        int passengerCount)
    {
        decimal classMultiplier = fareClass == FareClass.Executive ? ExecutiveMultiplier : EconomyMultiplier;
        decimal baseAmount = basePriceEconomy * classMultiplier * passengerCount;
        decimal taxes = baseAmount * TaxRate + TaxPerPassenger * passengerCount;
        decimal serviceFee = (baseAmount + taxes) * ServiceFeeRate;

        return new PriceCalculationResult
        {
            BaseAmount = baseAmount,
            Taxes = taxes,
            ServiceFee = serviceFee,
            PaymentAdjustment = 0m,
            TotalAmount = baseAmount + taxes + serviceFee
        };
    }

    public PriceCalculationResult CalculatePrice(
        decimal basePriceEconomy,
        FareClass fareClass,
        int passengerCount,
        PaymentMethod paymentMethod)
    {
        var pre = CalculatePrePaymentPrice(basePriceEconomy, fareClass, passengerCount);

        decimal paymentRate = paymentMethod switch
        {
            PaymentMethod.CreditCard => CreditCardRate,
            PaymentMethod.Pix => PixRate,
            PaymentMethod.Boleto => BoletoRate,
            _ => 0m
        };

        decimal paymentAdjustment = pre.TotalAmount * paymentRate;

        return new PriceCalculationResult
        {
            BaseAmount = pre.BaseAmount,
            Taxes = pre.Taxes,
            ServiceFee = pre.ServiceFee,
            PaymentAdjustment = paymentAdjustment,
            TotalAmount = pre.TotalAmount + paymentAdjustment
        };
    }
}
