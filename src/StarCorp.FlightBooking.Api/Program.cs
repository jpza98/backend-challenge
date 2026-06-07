using System.Text.Json.Serialization;
using StarCorp.FlightBooking.Core.Interfaces;
using StarCorp.FlightBooking.Core.Services;
using StarCorp.FlightBooking.Infrastructure;
using StarCorp.FlightBooking.Infrastructure.Repositories;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers()
    .AddJsonOptions(o =>
        o.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter()));

builder.Services.AddOpenApi();

var connectionString = builder.Configuration.GetConnectionString("DefaultConnection")
    ?? throw new InvalidOperationException("Connection string 'DefaultConnection' not found.");

DapperConfig.RegisterTypeHandlers();

builder.Services.AddScoped<IFlightRepository>(_   => new FlightRepository(connectionString));
builder.Services.AddScoped<IBookingRepository>(_  => new BookingRepository(connectionString));
builder.Services.AddScoped<ICustomerRepository>(_ => new CustomerRepository(connectionString));
builder.Services.AddScoped<IPricingService,      PricingService>();
builder.Services.AddScoped<ICancellationService, CancellationService>();

var app = builder.Build();

if (app.Environment.IsDevelopment())
    app.MapOpenApi();

app.UseHttpsRedirection();
app.MapControllers();
app.Run();
