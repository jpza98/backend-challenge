IF NOT EXISTS (SELECT * FROM sysobjects WHERE name='Airlines' AND xtype='U')
BEGIN
    CREATE TABLE Airlines (
        Id       INT           IDENTITY(1,1) PRIMARY KEY,
        Name     NVARCHAR(100) NOT NULL,
        IataCode CHAR(2)       NOT NULL,
        Country  NVARCHAR(100) NOT NULL,
        CONSTRAINT UQ_Airlines_IataCode UNIQUE (IataCode)
    );
END

IF NOT EXISTS (SELECT * FROM sysobjects WHERE name='Flights' AND xtype='U')
BEGIN
    CREATE TABLE Flights (
        Id                      INT           IDENTITY(1,1) PRIMARY KEY,
        FlightNumber            NVARCHAR(10)  NOT NULL,
        AirlineId               INT           NOT NULL,
        Origin                  NVARCHAR(3)   NOT NULL,
        Destination             NVARCHAR(3)   NOT NULL,
        DepartureTime           DATETIME2     NOT NULL,
        ArrivalTime             DATETIME2     NOT NULL,
        BasePriceEconomy        DECIMAL(10,2) NOT NULL,
        AvailableSeatsEconomy   INT           NOT NULL DEFAULT 0,
        AvailableSeatsExecutive INT           NOT NULL DEFAULT 0,
        Status                  NVARCHAR(20)  NOT NULL DEFAULT 'Scheduled',
        CONSTRAINT FK_Flights_Airlines FOREIGN KEY (AirlineId) REFERENCES Airlines(Id)
    );
END

IF NOT EXISTS (SELECT * FROM sysobjects WHERE name='Customers' AND xtype='U')
BEGIN
    CREATE TABLE Customers (
        Id        INT           IDENTITY(1,1) PRIMARY KEY,
        Name      NVARCHAR(200) NOT NULL,
        Email     NVARCHAR(200) NOT NULL,
        CPF       NVARCHAR(11)  NOT NULL,
        Phone     NVARCHAR(20)  NULL,
        CreatedAt DATETIME2     NOT NULL DEFAULT GETUTCDATE(),
        CONSTRAINT UQ_Customers_Email UNIQUE (Email),
        CONSTRAINT UQ_Customers_CPF   UNIQUE (CPF)
    );
END

IF NOT EXISTS (SELECT * FROM sysobjects WHERE name='Bookings' AND xtype='U')
BEGIN
    CREATE TABLE Bookings (
        Id          INT           IDENTITY(1,1) PRIMARY KEY,
        BookingCode NVARCHAR(8)   NOT NULL,
        CustomerId  INT           NOT NULL,
        FlightId    INT           NOT NULL,
        FareClass   NVARCHAR(20)  NOT NULL,
        Status      NVARCHAR(20)  NOT NULL DEFAULT 'Pending',
        TotalAmount DECIMAL(10,2) NOT NULL,
        CreatedAt   DATETIME2     NOT NULL DEFAULT GETUTCDATE(),
        CONSTRAINT UQ_Bookings_BookingCode UNIQUE (BookingCode),
        CONSTRAINT FK_Bookings_Customers   FOREIGN KEY (CustomerId) REFERENCES Customers(Id),
        CONSTRAINT FK_Bookings_Flights     FOREIGN KEY (FlightId)   REFERENCES Flights(Id)
    );
END

IF NOT EXISTS (SELECT * FROM sysobjects WHERE name='Passengers' AND xtype='U')
BEGIN
    CREATE TABLE Passengers (
        Id         INT           IDENTITY(1,1) PRIMARY KEY,
        BookingId  INT           NOT NULL,
        Name       NVARCHAR(200) NOT NULL,
        CPF        NVARCHAR(11)  NOT NULL,
        BirthDate  DATE          NOT NULL,
        SeatNumber NVARCHAR(5)   NULL,
        CONSTRAINT FK_Passengers_Bookings FOREIGN KEY (BookingId) REFERENCES Bookings(Id)
    );
END

IF NOT EXISTS (SELECT * FROM sysobjects WHERE name='Payments' AND xtype='U')
BEGIN
    CREATE TABLE Payments (
        Id            INT           IDENTITY(1,1) PRIMARY KEY,
        BookingId     INT           NOT NULL,
        Method        NVARCHAR(20)  NOT NULL,
        Amount        DECIMAL(10,2) NOT NULL,
        Status        NVARCHAR(20)  NOT NULL DEFAULT 'Pending',
        PaidAt        DATETIME2     NULL,
        TransactionId NVARCHAR(100) NULL,
        CONSTRAINT UQ_Payments_BookingId    UNIQUE (BookingId),
        CONSTRAINT FK_Payments_Bookings FOREIGN KEY (BookingId) REFERENCES Bookings(Id)
    );
END
