using System.Data;
using Dapper;
using Microsoft.Data.SqlClient;
using StarCorp.FlightBooking.Core.Enums;
using StarCorp.FlightBooking.Core.Interfaces;
using StarCorp.FlightBooking.Core.Models;

namespace StarCorp.FlightBooking.Infrastructure.Repositories;

public class BookingRepository : IBookingRepository
{
    private readonly string _connectionString;

    public BookingRepository(string connectionString) =>
        _connectionString = connectionString;

    private IDbConnection CreateConnection() => new SqlConnection(_connectionString);

    public Task<Booking?> GetByIdAsync(int id) =>
        LoadAsync("WHERE b.Id = @Param", new { Param = id });

    public Task<Booking?> GetByCodeAsync(string code) =>
        LoadAsync("WHERE b.BookingCode = @Param", new { Param = code });

    private async Task<Booking?> LoadAsync(string whereClause, object parameters)
    {
        using var conn = CreateConnection();

        var booking = await conn.QueryFirstOrDefaultAsync<Booking>(
            $"SELECT * FROM Bookings b {whereClause}", parameters);

        if (booking is null) return null;

        booking.Customer = await conn.QueryFirstOrDefaultAsync<Customer>(
            "SELECT * FROM Customers WHERE Id = @Id",
            new { Id = booking.CustomerId });

        var flight = await conn.QueryFirstOrDefaultAsync<Flight>(
            "SELECT * FROM Flights WHERE Id = @Id",
            new { Id = booking.FlightId });

        if (flight is not null)
        {
            flight.Airline = await conn.QueryFirstOrDefaultAsync<Airline>(
                "SELECT * FROM Airlines WHERE Id = @Id",
                new { Id = flight.AirlineId });
            booking.Flight = flight;
        }

        booking.Passengers = (await conn.QueryAsync<Passenger>(
            "SELECT * FROM Passengers WHERE BookingId = @BookingId",
            new { BookingId = booking.Id })).ToList();

        booking.Payment = await conn.QueryFirstOrDefaultAsync<Payment>(
            "SELECT * FROM Payments WHERE BookingId = @BookingId",
            new { BookingId = booking.Id });

        return booking;
    }

    public async Task<IEnumerable<Booking>> GetByCustomerIdAsync(int customerId)
    {
        using var conn = CreateConnection();
        return await conn.QueryAsync<Booking>(
            "SELECT * FROM Bookings WHERE CustomerId = @CustomerId ORDER BY CreatedAt DESC",
            new { CustomerId = customerId });
    }

    public async Task<Booking> CreateAsync(Booking booking)
    {
        const string insertBooking = """
            INSERT INTO Bookings (BookingCode, CustomerId, FlightId, FareClass, Status, TotalAmount, CreatedAt)
            OUTPUT INSERTED.Id
            VALUES (@BookingCode, @CustomerId, @FlightId, @FareClass, @Status, @TotalAmount, @CreatedAt)
            """;

        const string insertPassenger = """
            INSERT INTO Passengers (BookingId, Name, CPF, BirthDate, SeatNumber)
            VALUES (@BookingId, @Name, @CPF, @BirthDate, @SeatNumber)
            """;

        const string insertPayment = """
            INSERT INTO Payments (BookingId, Method, Amount, Status, PaidAt, TransactionId)
            VALUES (@BookingId, @Method, @Amount, @Status, @PaidAt, @TransactionId)
            """;

        using var conn = CreateConnection();
        conn.Open();
        using var tx = conn.BeginTransaction();

        try
        {
            booking.CreatedAt = DateTime.UtcNow;

            booking.Id = await conn.ExecuteScalarAsync<int>(insertBooking, new
            {
                booking.BookingCode,
                booking.CustomerId,
                booking.FlightId,
                FareClass = booking.FareClass.ToString(),
                Status = booking.Status.ToString(),
                booking.TotalAmount,
                booking.CreatedAt
            }, tx);

            foreach (var p in booking.Passengers)
            {
                p.BookingId = booking.Id;
                await conn.ExecuteAsync(insertPassenger, p, tx);
            }

            if (booking.Payment is not null)
            {
                booking.Payment.BookingId = booking.Id;
                await conn.ExecuteAsync(insertPayment, new
                {
                    booking.Payment.BookingId,
                    Method = booking.Payment.Method.ToString(),
                    booking.Payment.Amount,
                    booking.Payment.Status,
                    booking.Payment.PaidAt,
                    booking.Payment.TransactionId
                }, tx);
            }

            tx.Commit();
            return booking;
        }
        catch
        {
            tx.Rollback();
            throw;
        }
    }

    public async Task<bool> UpdateStatusAsync(int id, BookingStatus status)
    {
        using var conn = CreateConnection();
        var rows = await conn.ExecuteAsync(
            "UPDATE Bookings SET Status = @Status WHERE Id = @Id",
            new { Status = status.ToString(), Id = id });
        return rows > 0;
    }

    public async Task AddPaymentAsync(Payment payment)
    {
        const string sql = """
            INSERT INTO Payments (BookingId, Method, Amount, Status, PaidAt, TransactionId)
            VALUES (@BookingId, @Method, @Amount, @Status, @PaidAt, @TransactionId)
            """;

        using var conn = CreateConnection();
        await conn.ExecuteAsync(sql, new
        {
            payment.BookingId,
            Method = payment.Method.ToString(),
            payment.Amount,
            payment.Status,
            payment.PaidAt,
            payment.TransactionId
        });
    }

    public async Task<bool> UpdateTotalAmountAsync(int bookingId, decimal totalAmount)
    {
        using var conn = CreateConnection();
        var rows = await conn.ExecuteAsync(
            "UPDATE Bookings SET TotalAmount = @TotalAmount WHERE Id = @Id",
            new { TotalAmount = totalAmount, Id = bookingId });
        return rows > 0;
    }
}
