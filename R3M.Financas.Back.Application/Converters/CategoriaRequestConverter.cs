using R3M.Financas.Back.Domain.Dtos;
using R3M.Financas.Back.Domain.Models;

namespace R3M.Financas.Back.Application.Converters;

public class CategoriaRequestConverter : ConverterBase<CategoriaRequest, Categoria>
{
    public override Categoria Convert(CategoriaRequest dto)
    {
        return new Categoria
        {
            Nome = dto.Nome,
            ParentId = dto.ParentId
        };
    }

    public override CategoriaRequest Convert(Categoria domain)
    {
        return new CategoriaRequest
        {
            Nome = domain.Nome,
            ParentId = domain.ParentId
        };
    }
}
