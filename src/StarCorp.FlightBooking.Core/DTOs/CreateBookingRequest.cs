using System.ComponentModel.DataAnnotations;
using StarCorp.FlightBooking.Core.Enums;

namespace StarCorp.FlightBooking.Core.DTOs;

public class CreateBookingRequest
{
<<<<<<< HEAD
    [Required]
    public int FlightId { get; set; }

    [Required]
    public FareClass FareClass { get; set; }

    [Required]
    public CustomerRequest Customer { get; set; } = null!;
=======
        [Required]
        public int FlightId { get; set; }

        [Required]
        public FareClass FareClass { get; set; }

        [Required]
        public CustomerRequest Customer { get; set; } = null!;
>>>>>>> 9cff74b0823685c7d7cc1aa676ab89b47311490f

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
<<<<<<< HEAD
    [Required] public string Name { get; set; } = string.Empty;
    [Required] public string Email { get; set; } = string.Empty;
    [Required] public string CPF { get; set; } = string.Empty;
    public string? Phone { get; set; }
=======
        [Required]
        public string Name { get; set; } = string.Empty;

        [Required]
        public string Email { get; set; } = string.Empty;

        [Required]
        public string CPF { get; set; } = string.Empty;

        public string? Phone { get; set; }
>>>>>>> 9cff74b0823685c7d7cc1aa676ab89b47311490f
}

public class PassengerRequest
{
<<<<<<< HEAD
    [Required] public string Name { get; set; } = string.Empty;
    [Required] public string CPF { get; set; } = string.Empty;
    [Required] public DateTime BirthDate { get; set; }
=======
        [Required]
        public string Name { get; set; } = string.Empty;

        [Required]
        public string CPF { get; set; } = string.Empty;

        [Required]
        public DateTime BirthDate { get; set; }
>>>>>>> 9cff74b0823685c7d7cc1aa676ab89b47311490f
}
