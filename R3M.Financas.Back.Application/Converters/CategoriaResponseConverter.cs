using R3M.Financas.Back.Application.Interfaces;
using R3M.Financas.Back.Domain.Dtos;
using R3M.Financas.Back.Domain.Models;

namespace R3M.Financas.Back.Application.Converters;

public class CategoriaResponseConverter : ConverterBase<CategoriaResponse, Categoria>
{
    public override Categoria Convert(CategoriaResponse dto)
    {
        return new Categoria
        {
            Id = dto.CategoriaId,
            Nome = dto.Nome,
            ParentId = dto.ParentId
        };
    }

    public override CategoriaResponse Convert(Categoria domain)
    {
        return new CategoriaResponse
        {
            CategoriaId = domain.Id,
            Nome = domain.Nome,
            ParentId = domain.ParentId
        };
    }
}
