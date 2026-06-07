using StarCorp.FlightBooking.Core.DTOs;
using StarCorp.FlightBooking.Core.Enums;
using StarCorp.FlightBooking.Core.Interfaces;
using StarCorp.FlightBooking.Core.Models;

namespace StarCorp.FlightBooking.Core.Services;

public class CancellationService : ICancellationService
{
    public bool CanCancel(Booking booking) =>
        booking.Status is BookingStatus.Pending or BookingStatus.Confirmed;

    public RefundResult CalculateRefund(Booking booking, DateTime cancellationTime)
    {
        if (!CanCancel(booking))
            return RefundResult.Failure("Reserva não pode ser cancelada no status atual.");

        if (booking.Flight is null)
            throw new InvalidOperationException("Booking.Flight deve estar carregado para calcular o reembolso.");

        // Regra dos 24h após pagamento: reembolso integral independente de qualquer outra condição
        if (booking.Payment?.PaidAt is not null)
        {
            var hoursSincePayment = (cancellationTime - booking.Payment.PaidAt.Value).TotalHours;
            if (hoursSincePayment <= 24)
                return RefundResult.Success(booking.TotalAmount, 1.0m,
                    "Cancelamento dentro de 24h após o pagamento. Reembolso integral.");
        }

        double daysUntilFlight = (booking.Flight.DepartureTime - cancellationTime).TotalDays;

        return booking.FareClass == FareClass.Economy
            ? CalculateEconomyRefund(booking.TotalAmount, daysUntilFlight)
            : CalculateExecutiveRefund(booking.TotalAmount, daysUntilFlight);
    }

    private static RefundResult CalculateEconomyRefund(decimal totalAmount, double daysUntilFlight)
    {
        if (daysUntilFlight > 7)
            return RefundResult.Success(totalAmount, 1.0m,
                "Econômica: cancelamento com mais de 7 dias. Reembolso integral.");

        if (daysUntilFlight >= 2)
            return RefundResult.Success(totalAmount * 0.5m, 0.5m,
                "Econômica: cancelamento entre 2 e 7 dias. Reembolso de 50%.");

        return RefundResult.Success(0m, 0m,
            "Econômica: cancelamento com menos de 2 dias. Sem reembolso.");
    }

    private static RefundResult CalculateExecutiveRefund(decimal totalAmount, double daysUntilFlight)
    {
        if (daysUntilFlight > 7)
            return RefundResult.Success(totalAmount, 1.0m,
                "Executiva: cancelamento com mais de 7 dias. Reembolso integral.");

        if (daysUntilFlight >= 2)
            return RefundResult.Success(totalAmount * 0.75m, 0.75m,
                "Executiva: cancelamento entre 2 e 7 dias. Reembolso de 75%.");

        return RefundResult.Success(totalAmount * 0.25m, 0.25m,
            "Executiva: cancelamento com menos de 2 dias. Reembolso de 25%.");
    }
}
