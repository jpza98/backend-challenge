using System.Data;
using Dapper;
using StarCorp.FlightBooking.Core.Enums;

namespace StarCorp.FlightBooking.Infrastructure;

public static class DapperConfig
{
    public static void RegisterTypeHandlers()
    {
        SqlMapper.AddTypeHandler(new EnumTypeHandler<FareClass>());
        SqlMapper.AddTypeHandler(new EnumTypeHandler<BookingStatus>());
        SqlMapper.AddTypeHandler(new EnumTypeHandler<PaymentMethod>());
    }

    private sealed class EnumTypeHandler<T> : SqlMapper.TypeHandler<T> where T : struct, Enum
    {
        public override void SetValue(IDbDataParameter parameter, T value) =>
            parameter.Value = value.ToString();

        public override T Parse(object value) =>
            Enum.Parse<T>(value.ToString()!, ignoreCase: true);
    }
}
