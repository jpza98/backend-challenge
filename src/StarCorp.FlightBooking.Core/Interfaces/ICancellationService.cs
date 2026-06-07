using StarCorp.FlightBooking.Core.DTOs;
using StarCorp.FlightBooking.Core.Models;

namespace StarCorp.FlightBooking.Core.Interfaces;

public interface ICancellationService
{
    RefundResult CalculateRefund(Booking booking, DateTime cancellationTime);
    bool CanCancel(Booking booking);
}
