using System.ComponentModel.DataAnnotations;
using StarCorp.FlightBooking.Core.Enums;

namespace StarCorp.FlightBooking.Core.DTOs;

public class CreateBookingRequest
{
    [Required]
    public int FlightId { get; set; }

    [Required]
    public FareClass FareClass { get; set; }

    [Required]
    public CustomerRequest Customer { get; set; } = null!;

    [Required]
    [MinLength(1, ErrorMessage = "At least one passenger is required.")]
    public List<PassengerRequest> Passengers { get; set; } = [];
}

public class ProcessPaymentRequest
{
    [Required]
    public PaymentMethod PaymentMethod { get; set; }
}

public class CustomerRequest
{
    [Required] public string Name { get; set; } = string.Empty;
    [Required] public string Email { get; set; } = string.Empty;
    [Required] public string CPF { get; set; } = string.Empty;
    public string? Phone { get; set; }
}

public class PassengerRequest
{
    [Required] public string Name { get; set; } = string.Empty;
    [Required] public string CPF { get; set; } = string.Empty;
    [Required] public DateTime BirthDate { get; set; }
}
