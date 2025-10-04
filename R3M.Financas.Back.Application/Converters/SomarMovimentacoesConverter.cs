using R3M.Financas.Back.Application.Interfaces;
using R3M.Financas.Back.Domain.Dtos;
using R3M.Financas.Back.Domain.Models;

namespace R3M.Financas.Back.Application.Converters;

public class SomarMovimentacoesConverter
    : ConverterBase<SomarMovimentacoesResponse, SomarMovimentacoesDto>
{
    private readonly IConverter<CategoriaResponse, Categoria> categoriaResponseConverter;

    public SomarMovimentacoesConverter(IConverter<CategoriaResponse, Categoria> categoriaResponseConverter)
    {
        this.categoriaResponseConverter = categoriaResponseConverter;
    }

    public override SomarMovimentacoesDto Convert(SomarMovimentacoesResponse dto)
    {
        if (dto == null) return null;

        return new SomarMovimentacoesDto
        {
            Id = dto.Categoria?.CategoriaId ?? Guid.Empty,
            Nome = dto.Categoria?.Nome,
            Valor = dto.Valor
        };
    }

    public override SomarMovimentacoesResponse Convert(SomarMovimentacoesDto domain)
    {
        if (domain == null) return null;

        return new SomarMovimentacoesResponse
        {
            Categoria = new CategoriaResponse
            {
                CategoriaId = domain.Id,
                Nome = domain.Nome
            },
            Valor = domain.Valor
        };
    }
}
