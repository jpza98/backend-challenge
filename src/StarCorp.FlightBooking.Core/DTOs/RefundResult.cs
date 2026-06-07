namespace StarCorp.FlightBooking.Core.DTOs;

public class RefundResult
{
    public bool IsSuccess { get; init; }
    public decimal RefundAmount { get; init; }
    public decimal RefundPercentage { get; init; }
    public string Reason { get; init; } = string.Empty;

    public static RefundResult Success(decimal amount, decimal percentage, string reason) => new()
    {
        IsSuccess = true,
        RefundAmount = amount,
        RefundPercentage = percentage,
        Reason = reason
    };

    public static RefundResult Failure(string reason) => new()
    {
        IsSuccess = false,
        RefundAmount = 0m,
        RefundPercentage = 0m,
        Reason = reason
    };
}
