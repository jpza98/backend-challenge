using System.Data;
using Dapper;
using Microsoft.Data.SqlClient;
using StarCorp.FlightBooking.Core.Interfaces;
using StarCorp.FlightBooking.Core.Models;

namespace StarCorp.FlightBooking.Infrastructure.Repositories;

public class CustomerRepository : ICustomerRepository
{
    private readonly string _connectionString;

    public CustomerRepository(string connectionString) =>
        _connectionString = connectionString;

    private IDbConnection CreateConnection() => new SqlConnection(_connectionString);

    public async Task<Customer?> GetByIdAsync(int id)
    {
        using var conn = CreateConnection();
        return await conn.QueryFirstOrDefaultAsync<Customer>(
            "SELECT * FROM Customers WHERE Id = @Id", new { Id = id });
    }

    public async Task<Customer?> GetByEmailAsync(string email)
    {
        using var conn = CreateConnection();
        return await conn.QueryFirstOrDefaultAsync<Customer>(
            "SELECT * FROM Customers WHERE Email = @Email", new { Email = email });
    }

    public async Task<Customer?> GetByCPFAsync(string cpf)
    {
        using var conn = CreateConnection();
        return await conn.QueryFirstOrDefaultAsync<Customer>(
            "SELECT * FROM Customers WHERE CPF = @CPF", new { CPF = cpf });
    }

    public async Task<Customer> CreateAsync(Customer customer)
    {
        const string sql = """
            INSERT INTO Customers (Name, Email, CPF, Phone, CreatedAt)
            OUTPUT INSERTED.Id
            VALUES (@Name, @Email, @CPF, @Phone, @CreatedAt)
            """;

        using var conn = CreateConnection();
        customer.CreatedAt = DateTime.UtcNow;
        customer.Id = await conn.ExecuteScalarAsync<int>(sql, customer);
        return customer;
    }
}
