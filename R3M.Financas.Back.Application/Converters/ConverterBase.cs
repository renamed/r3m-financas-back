using R3M.Financas.Back.Application.Interfaces;

namespace R3M.Financas.Back.Application.Converters;

public abstract class ConverterBase<TDto, TDomain> : IConverter<TDto, TDomain>
{
    public IEnumerable<TDomain> BulkConvert(IEnumerable<TDto> dtos)
    {
        foreach (var item in dtos)
        {
            yield return Convert(item);
        }
    }

    public IEnumerable<TDto> BulkConvert(IEnumerable<TDomain> domains)
    {
        foreach (var item in domains)
        {
            yield return Convert(item);
        }
    }

    public abstract TDomain Convert(TDto dto);
    public abstract TDto Convert(TDomain domain);
}
