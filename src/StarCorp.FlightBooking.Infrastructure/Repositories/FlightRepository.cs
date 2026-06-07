using System.Data;
using Dapper;
using Microsoft.Data.SqlClient;
using StarCorp.FlightBooking.Core.Enums;
using StarCorp.FlightBooking.Core.Interfaces;
using StarCorp.FlightBooking.Core.Models;

namespace StarCorp.FlightBooking.Infrastructure.Repositories;

public class FlightRepository : IFlightRepository
{
    private readonly string _connectionString;

    public FlightRepository(string connectionString) =>
        _connectionString = connectionString;

    private IDbConnection CreateConnection() => new SqlConnection(_connectionString);

    public async Task<Flight?> GetByIdAsync(int id)
    {
        const string sql = """
            SELECT f.Id, f.FlightNumber, f.AirlineId, f.Origin, f.Destination,
                   f.DepartureTime, f.ArrivalTime, f.BasePriceEconomy,
                   f.AvailableSeatsEconomy, f.AvailableSeatsExecutive, f.Status,
                   a.Id, a.Name, a.IataCode, a.Country
            FROM   Flights  f
            JOIN   Airlines a ON a.Id = f.AirlineId
            WHERE  f.Id = @Id
            """;

        using var conn = CreateConnection();
        var results = await conn.QueryAsync<Flight, Airline, Flight>(
            sql,
            (flight, airline) => { flight.Airline = airline; return flight; },
            new { Id = id },
            splitOn: "Id");

        return results.FirstOrDefault();
    }

    public async Task<IEnumerable<Flight>> SearchAsync(
        string? origin,
        string? destination,
        DateTime? date,
        FareClass? fareClass,
        int passengers)
    {
        var conditions = new List<string> { "f.Status = 'Scheduled'", "f.DepartureTime > GETUTCDATE()" };
        var parameters = new DynamicParameters();

        if (!string.IsNullOrWhiteSpace(origin))
        {
            conditions.Add("f.Origin = @Origin");
            parameters.Add("Origin", origin.ToUpperInvariant());
        }

        if (!string.IsNullOrWhiteSpace(destination))
        {
            conditions.Add("f.Destination = @Destination");
            parameters.Add("Destination", destination.ToUpperInvariant());
        }

        if (date.HasValue)
        {
            conditions.Add("CAST(f.DepartureTime AS DATE) = @Date");
            parameters.Add("Date", date.Value.Date);
        }

        if (fareClass == FareClass.Economy)
        {
            conditions.Add("f.AvailableSeatsEconomy >= @Passengers");
            parameters.Add("Passengers", passengers);
        }
        else if (fareClass == FareClass.Executive)
        {
            conditions.Add("f.AvailableSeatsExecutive >= @Passengers");
            parameters.Add("Passengers", passengers);
        }

        var sql = $"""
            SELECT f.Id, f.FlightNumber, f.AirlineId, f.Origin, f.Destination,
                   f.DepartureTime, f.ArrivalTime, f.BasePriceEconomy,
                   f.AvailableSeatsEconomy, f.AvailableSeatsExecutive, f.Status,
                   a.Id, a.Name, a.IataCode, a.Country
            FROM   Flights  f
            JOIN   Airlines a ON a.Id = f.AirlineId
            WHERE  {string.Join(" AND ", conditions)}
            ORDER  BY f.DepartureTime
            """;

        using var conn = CreateConnection();
        return await conn.QueryAsync<Flight, Airline, Flight>(
            sql,
            (flight, airline) => { flight.Airline = airline; return flight; },
            parameters,
            splitOn: "Id");
    }

    public async Task<bool> UpdateSeatsAsync(int flightId, FareClass fareClass, int delta)
    {
        var column = fareClass == FareClass.Economy ? "AvailableSeatsEconomy" : "AvailableSeatsExecutive";
        var sql = $"""
            UPDATE Flights
            SET    {column} = {column} + @Delta
            WHERE  Id = @Id
              AND  {column} + @Delta >= 0
            """;

        using var conn = CreateConnection();
        return await conn.ExecuteAsync(sql, new { Delta = delta, Id = flightId }) > 0;
    }
}
