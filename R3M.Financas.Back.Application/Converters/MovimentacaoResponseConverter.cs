using R3M.Financas.Back.Application.Interfaces;
using R3M.Financas.Back.Domain.Dtos;
using R3M.Financas.Back.Domain.Models;

namespace R3M.Financas.Back.Application.Converters;

public class MovimentacaoResponseConverter
    : ConverterBase<MovimentacaoResponse, Movimentacao>
{

    private readonly IConverter<CategoriaResponse, Categoria> categoriaConverter;
    private readonly IConverter<InstituicaoResponse, Instituicao> instituicaoConverter;
    private readonly IConverter<PeriodoResponse, Periodo> periodoConverter;

    public MovimentacaoResponseConverter(IConverter<CategoriaResponse, Categoria> categoriaConverter, IConverter<InstituicaoResponse, Instituicao> instituicaoConverter, IConverter<PeriodoResponse, Periodo> periodoConverter)
    {
        this.categoriaConverter = categoriaConverter;
        this.instituicaoConverter = instituicaoConverter;
        this.periodoConverter = periodoConverter;
    }

    public override Movimentacao Convert(MovimentacaoResponse dto)
    {
        return new Movimentacao
        {
            CategoriaId = dto.Categoria.CategoriaId,
            PeriodoId = dto.Periodo.PeriodoId,
            Categoria = new Categoria { Id = dto.Categoria.CategoriaId },
            Periodo = new Periodo { Id = dto.Periodo.PeriodoId },
            Data = dto.Data,
            Descricao = dto.Descricao,
            InstituicaoId = dto.Instituicao.InstituicaoId,
            Instituicao = new Instituicao { Id = dto.Instituicao.InstituicaoId },
            Valor = dto.Valor
        };
    }

    public override MovimentacaoResponse Convert(Movimentacao domain)
    {
        return new MovimentacaoResponse
        {
            MovimentacaoId = domain.Id,
            Categoria = categoriaConverter.Convert(domain.Categoria),
            Periodo = periodoConverter.Convert(domain.Periodo),
            Instituicao = instituicaoConverter.Convert(domain.Instituicao),
            Data = domain.Data,
            Descricao = domain.Descricao,
            Valor = domain.Valor
        };
    }
}
