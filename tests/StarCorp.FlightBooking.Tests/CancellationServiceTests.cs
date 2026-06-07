using StarCorp.FlightBooking.Core.Enums;
using StarCorp.FlightBooking.Core.Models;
using StarCorp.FlightBooking.Core.Services;

namespace StarCorp.FlightBooking.Tests;

public class CancellationServiceTests
{
    private readonly CancellationService _sut = new();

    // ─── Helpers ─────────────────────────────────────────────────────────────

    private static Booking MakeBooking(
        FareClass fareClass,
        BookingStatus status,
        double daysUntilFlight,
        DateTime? paidAt = null,
        decimal total = 1000m) => new()
        {
            Id = 1,
            FareClass = fareClass,
            Status = status,
            TotalAmount = total,
            Flight = new Flight
            {
                DepartureTime = DateTime.UtcNow.AddDays(daysUntilFlight)
            },
            Payment = paidAt.HasValue
            ? new Payment { PaidAt = paidAt }
            : null
        };

    // ─── CanCancel ────────────────────────────────────────────────────────────

    [Theory]
    [InlineData(BookingStatus.Pending)]
    [InlineData(BookingStatus.Confirmed)]
    public void CanCancel_ReturnsTrueFor_PendingOrConfirmed(BookingStatus status)
    {
        var booking = MakeBooking(FareClass.Economy, status, 10);
        Assert.True(_sut.CanCancel(booking));
    }

    [Theory]
    [InlineData(BookingStatus.Cancelled)]
    [InlineData(BookingStatus.Completed)]
    public void CanCancel_ReturnsFalseFor_CancelledOrCompleted(BookingStatus status)
    {
        var booking = MakeBooking(FareClass.Economy, status, 10);
        Assert.False(_sut.CanCancel(booking));
    }

    // ─── Regra das 24h ───────────────────────────────────────────────────────

    [Fact]
    public void Within24hOfPayment_FullRefund_IgnoringFareAndDays()
    {
        // Econômica, <2 dias → normalmente 0%, mas pagou há 12h
        var booking = MakeBooking(FareClass.Economy, BookingStatus.Confirmed,
            daysUntilFlight: 1, paidAt: DateTime.UtcNow.AddHours(-12));

        var result = _sut.CalculateRefund(booking, DateTime.UtcNow);

        Assert.True(result.IsSuccess);
        Assert.Equal(1.0m, result.RefundPercentage);
        Assert.Equal(1000m, result.RefundAmount);
    }

    [Fact]
    public void Exactly24hAfterPayment_StillFullRefund()
    {
        var paidAt = DateTime.UtcNow.AddHours(-24).AddMinutes(1); // 23h59 atrás
        var booking = MakeBooking(FareClass.Economy, BookingStatus.Confirmed,
            daysUntilFlight: 1, paidAt: paidAt);

        var result = _sut.CalculateRefund(booking, DateTime.UtcNow);

        Assert.True(result.IsSuccess);
        Assert.Equal(1.0m, result.RefundPercentage);
    }

    [Fact]
    public void After24hOfPayment_FallsBackToDateRules()
    {
        // Econômica, <2 dias, pagou há 25h → 0%
        var booking = MakeBooking(FareClass.Economy, BookingStatus.Confirmed,
            daysUntilFlight: 1, paidAt: DateTime.UtcNow.AddHours(-25));

        var result = _sut.CalculateRefund(booking, DateTime.UtcNow);

        Assert.True(result.IsSuccess);
        Assert.Equal(0m, result.RefundPercentage);
    }

    // ─── Econômica ────────────────────────────────────────────────────────────

    [Fact]
    public void Economy_MoreThan7Days_FullRefund()
    {
        var booking = MakeBooking(FareClass.Economy, BookingStatus.Confirmed, daysUntilFlight: 10);
        var result = _sut.CalculateRefund(booking, DateTime.UtcNow);

        Assert.True(result.IsSuccess);
        Assert.Equal(1.0m, result.RefundPercentage);
        Assert.Equal(1000m, result.RefundAmount);
    }

    [Theory]
    [InlineData(2.5)]
    [InlineData(4.5)]
    [InlineData(7.0)]
    public void Economy_TwoToSevenDays_FiftyPercentRefund(double days)
    {
        var booking = MakeBooking(FareClass.Economy, BookingStatus.Confirmed, daysUntilFlight: days);
        var result = _sut.CalculateRefund(booking, DateTime.UtcNow);

        Assert.True(result.IsSuccess);
        Assert.Equal(0.5m, result.RefundPercentage);
        Assert.Equal(500m, result.RefundAmount);
    }

    [Theory]
    [InlineData(0.0)]
    [InlineData(1.0)]
    [InlineData(1.99)]
    public void Economy_LessThan2Days_NoRefund(double days)
    {
        var booking = MakeBooking(FareClass.Economy, BookingStatus.Confirmed, daysUntilFlight: days);
        var result = _sut.CalculateRefund(booking, DateTime.UtcNow);

        Assert.True(result.IsSuccess);
        Assert.Equal(0m, result.RefundPercentage);
        Assert.Equal(0m, result.RefundAmount);
    }

    // ─── Executiva ────────────────────────────────────────────────────────────

    [Fact]
    public void Executive_MoreThan7Days_FullRefund()
    {
        var booking = MakeBooking(FareClass.Executive, BookingStatus.Confirmed, daysUntilFlight: 8);
        var result = _sut.CalculateRefund(booking, DateTime.UtcNow);

        Assert.True(result.IsSuccess);
        Assert.Equal(1.0m, result.RefundPercentage);
        Assert.Equal(1000m, result.RefundAmount);
    }

    [Theory]
    [InlineData(2.5)]
    [InlineData(5.0)]
    [InlineData(7.0)]
    public void Executive_TwoToSevenDays_SeventyFivePercentRefund(double days)
    {
        var booking = MakeBooking(FareClass.Executive, BookingStatus.Confirmed, daysUntilFlight: days);
        var result = _sut.CalculateRefund(booking, DateTime.UtcNow);

        Assert.True(result.IsSuccess);
        Assert.Equal(0.75m, result.RefundPercentage);
        Assert.Equal(750m, result.RefundAmount);
    }

    [Theory]
    [InlineData(0.0)]
    [InlineData(1.0)]
    [InlineData(1.99)]
    public void Executive_LessThan2Days_TwentyFivePercentRefund(double days)
    {
        var booking = MakeBooking(FareClass.Executive, BookingStatus.Confirmed, daysUntilFlight: days);
        var result = _sut.CalculateRefund(booking, DateTime.UtcNow);

        Assert.True(result.IsSuccess);
        Assert.Equal(0.25m, result.RefundPercentage);
        Assert.Equal(250m, result.RefundAmount);
    }

    // ─── Status inválido ──────────────────────────────────────────────────────

    [Fact]
    public void AlreadyCancelled_ReturnsFailure()
    {
        var booking = MakeBooking(FareClass.Economy, BookingStatus.Cancelled, daysUntilFlight: 10);
        var result = _sut.CalculateRefund(booking, DateTime.UtcNow);

        Assert.False(result.IsSuccess);
        Assert.Equal(0m, result.RefundAmount);
    }

    [Fact]
    public void FlightNotLoaded_ThrowsInvalidOperationException()
    {
        var booking = new Booking
        {
            Status = BookingStatus.Confirmed,
            FareClass = FareClass.Economy,
            TotalAmount = 500m,
            Flight = null
        };

        Assert.Throws<InvalidOperationException>(
            () => _sut.CalculateRefund(booking, DateTime.UtcNow));
    }
}
