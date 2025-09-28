namespace R3M.Financas.Back.Application.Interfaces;

public interface IConverter<TDto, TDomain>
{
    TDomain Convert(TDto request);
    TDto Convert(TDomain domain);

    IEnumerable<TDomain> BulkConvert(IEnumerable<TDto> request);
    IEnumerable<TDto> BulkConvert(IEnumerable<TDomain> request);
}
