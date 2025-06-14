using Dapper;
using System.Data;

namespace R3M.Financas.Back.Api.Data;
public class DateOnlyTypeHandler : SqlMapper.TypeHandler<DateOnly>
{
    public override void SetValue(IDbDataParameter parameter, DateOnly value)
    {
        parameter.Value = value.ToDateTime(TimeOnly.MinValue);
    }

    public override DateOnly Parse(object value)
    {
        return value switch
        {
            DateTime dateTime => DateOnly.FromDateTime(dateTime),
            _ => throw new InvalidCastException()
        };
    }
}

