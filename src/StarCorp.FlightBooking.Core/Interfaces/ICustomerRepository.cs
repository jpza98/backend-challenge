using StarCorp.FlightBooking.Core.Models;

namespace StarCorp.FlightBooking.Core.Interfaces;

public interface ICustomerRepository
{
    Task<Customer?> GetByIdAsync(int id);
    Task<Customer?> GetByEmailAsync(string email);
    Task<Customer?> GetByCPFAsync(string cpf);
    Task<Customer> CreateAsync(Customer customer);
}
