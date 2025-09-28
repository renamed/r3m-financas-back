using R3M.Financas.Back.Domain.Dtos;
using R3M.Financas.Back.Domain.Models;

namespace R3M.Financas.Back.Application.Converters;

public class TipoCategoriaConverter
    : ConverterBase<TipoCategoriaResponse, TipoCategoria>
{
    public override TipoCategoria Convert(TipoCategoriaResponse dto)
    {
        return new TipoCategoria
        {
            Id = dto.TipoCategoriaId,
            Nome = dto.Nome
        };
    }

    public override TipoCategoriaResponse Convert(TipoCategoria domain)
    {
        return new TipoCategoriaResponse
        {
            TipoCategoriaId = domain.Id,
            Nome = domain.Nome
        };
    }
}
