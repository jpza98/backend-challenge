USE StarCorpFlightBooking;
GO

DELETE FROM Payments;
DELETE FROM Passengers;
DELETE FROM Bookings;
DELETE FROM Customers;
DELETE FROM Flights;
DELETE FROM Airlines;

DBCC CHECKIDENT ('Airlines',  RESEED, 0);
DBCC CHECKIDENT ('Flights',   RESEED, 0);
DBCC CHECKIDENT ('Customers', RESEED, 0);
DBCC CHECKIDENT ('Bookings',  RESEED, 0);
DBCC CHECKIDENT ('Passengers',RESEED, 0);
DBCC CHECKIDENT ('Payments',  RESEED, 0);

INSERT INTO Airlines (Name, IataCode, Country) VALUES
('LATAM Airlines Brasil',            'LA', 'Brazil'),
('Gol Linhas Aéreas',                'G3', 'Brazil'),
('Azul Linhas Aéreas Brasileiras',   'AD', 'Brazil');

INSERT INTO Flights
    (FlightNumber, AirlineId, Origin, Destination, DepartureTime,          ArrivalTime,             BasePriceEconomy, AvailableSeatsEconomy, AvailableSeatsExecutive, Status)
VALUES
('LA3421', 1, 'GRU', 'SSA', '2026-06-20 08:00:00', '2026-06-20 10:30:00', 450.00, 120, 20, 'Scheduled'),
('G3542',  2, 'GRU', 'CGH', '2026-06-25 14:00:00', '2026-06-25 15:00:00', 220.00, 150, 15, 'Scheduled'),
('AD7821', 3, 'VCP', 'REC', '2026-07-01 06:00:00', '2026-07-01 09:30:00', 580.00, 100, 10, 'Scheduled'),
('LA1234', 1, 'GIG', 'GRU', '2026-06-15 07:00:00', '2026-06-15 08:00:00', 280.00, 180, 25, 'Scheduled'),
('G3678',  2, 'BSB', 'FOR', '2026-07-10 11:00:00', '2026-07-10 14:30:00', 620.00,  90, 12, 'Scheduled');

INSERT INTO Customers (Name, Email, CPF, Phone, IsActive) VALUES
('João Silva',   'joao.silva@email.com',   '12345678901', '11999991111', 1),
('Maria Santos', 'maria.santos@email.com', '98765432100', '21999992222', 1),
('Pedro Costa',  'pedro.costa@email.com',  '11122233344', '31999993333', 0); -- inativo para testar 422
