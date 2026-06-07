using StarCorp.FlightBooking.Core.Enums;
using StarCorp.FlightBooking.Core.Services;

namespace StarCorp.FlightBooking.Tests;

public class PricingServiceTests
{
    private readonly PricingService _sut = new();

    // ─── Fórmula base ────────────────────────────────────────────────────────
    // Base      = priceEconomy × classMultiplier × passengers
    // Taxes     = Base × 0.08  +  45 × passengers
    // ServiceFee= (Base + Taxes) × 0.05
    // SubTotal  = Base + Taxes + ServiceFee
    // Total     = SubTotal + SubTotal × paymentRate

    [Fact]
    public void Economy_OnePassenger_Pix_CalculatesCorrectTotal()
    {
        // Base  = 500 × 1.0 × 1 = 500
        // Taxes = 500×0.08 + 45  = 40 + 45 = 85
        // SvcFee= (500+85)×0.05  = 29.25
        // Sub   = 500+85+29.25   = 614.25
        // Pix   = 614.25×(-0.05) = -30.7125
        // Total = 614.25 - 30.7125 = 583.5375
        var result = _sut.CalculatePrice(500m, FareClass.Economy, 1, PaymentMethod.Pix);

        Assert.Equal(500m,      result.BaseAmount);
        Assert.Equal(85m,       result.Taxes);
        Assert.Equal(29.25m,    result.ServiceFee);
        Assert.Equal(-30.7125m, result.PaymentAdjustment);
        Assert.Equal(583.5375m, result.TotalAmount);
    }

    [Fact]
    public void Economy_OnePassenger_CreditCard_AppliesThreePercentSurcharge()
    {
        var result = _sut.CalculatePrice(500m, FareClass.Economy, 1, PaymentMethod.CreditCard);

        // SubTotal = 614.25, CreditCard = +3%
        Assert.Equal(614.25m * 0.03m, result.PaymentAdjustment);
        Assert.Equal(614.25m + 614.25m * 0.03m, result.TotalAmount);
    }

    [Fact]
    public void Economy_OnePassenger_Boleto_AppliesOnePercentSurcharge()
    {
        var result = _sut.CalculatePrice(500m, FareClass.Economy, 1, PaymentMethod.Boleto);

        Assert.Equal(614.25m * 0.01m, result.PaymentAdjustment);
        Assert.Equal(614.25m + 614.25m * 0.01m, result.TotalAmount);
    }

    [Fact]
    public void Executive_HasTwoPointFiveMultiplier()
    {
        var economy   = _sut.CalculatePrice(400m, FareClass.Economy,   1, PaymentMethod.Pix);
        var executive = _sut.CalculatePrice(400m, FareClass.Executive, 1, PaymentMethod.Pix);

        // Base executiva deve ser exatamente 2.5× a econômica
        Assert.Equal(economy.BaseAmount * 2.5m, executive.BaseAmount);
    }

    [Fact]
    public void MultiplePassengers_ScalesBaseAndTaxesCorrectly()
    {
        // 3 passageiros, Econômica, preço base = 300
        // Base  = 300 × 1 × 3 = 900
        // Taxes = 900×0.08 + 45×3 = 72 + 135 = 207
        var result = _sut.CalculatePrice(300m, FareClass.Economy, 3, PaymentMethod.Pix);

        Assert.Equal(900m, result.BaseAmount);
        Assert.Equal(207m, result.Taxes);
    }

    [Fact]
    public void TaxPerPassenger_IsFortyFiveReais()
    {
        var one = _sut.CalculatePrice(1000m, FareClass.Economy, 1, PaymentMethod.Pix);
        var two = _sut.CalculatePrice(1000m, FareClass.Economy, 2, PaymentMethod.Pix);

        // Diferença de imposto deve incluir 45 extra por passageiro adicional
        Assert.True(two.Taxes - one.Taxes > 45m);
    }

    [Fact]
    public void ServiceFee_IsFivePercentOfBaseAndTaxes()
    {
        var result = _sut.CalculatePrice(600m, FareClass.Economy, 2, PaymentMethod.Pix);

        var expectedFee = (result.BaseAmount + result.Taxes) * 0.05m;
        Assert.Equal(expectedFee, result.ServiceFee);
    }

    [Fact]
    public void TotalAmount_EqualsSumOfAllComponents()
    {
        var result = _sut.CalculatePrice(750m, FareClass.Executive, 2, PaymentMethod.CreditCard);

        var expected = result.BaseAmount + result.Taxes + result.ServiceFee + result.PaymentAdjustment;
        Assert.Equal(expected, result.TotalAmount);
    }

    [Fact]
    public void Pix_IsAlwaysCheaperThan_CreditCard_ForSameFlight()
    {
        var pix    = _sut.CalculatePrice(500m, FareClass.Economy, 1, PaymentMethod.Pix);
        var credit = _sut.CalculatePrice(500m, FareClass.Economy, 1, PaymentMethod.CreditCard);

        Assert.True(pix.TotalAmount < credit.TotalAmount);
    }
}
