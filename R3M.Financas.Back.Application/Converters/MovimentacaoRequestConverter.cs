using R3M.Financas.Back.Application.Interfaces;
using R3M.Financas.Back.Domain.Dtos;
using R3M.Financas.Back.Domain.Models;

namespace R3M.Financas.Back.Application.Converters;

public class MovimentacaoRequestConverter
    : ConverterBase<MovimentacaoRequest, Movimentacao>
{
    //public Periodo Periodo { get; set; }
    //public Instituicao Instituicao { get; set; }
    //public Categoria Categoria { get; set; }

    //private readonly IConverter<CategoriaResponse, Categoria> categoriaConverter;
    //private readonly IConverter<InstituicaoResponse, Instituicao> instituicaoConverter;
    //private readonly IConverter<PeriodoResponse, Periodo> periodoConverter;

    //public MovimentacaoRequestConverter(IConverter<CategoriaResponse, Categoria> categoriaConverter, IConverter<InstituicaoResponse, Instituicao> instituicaoConverter, IConverter<PeriodoResponse, Periodo> periodoConverter)
    //{
    //    this.categoriaConverter = categoriaConverter;
    //    this.instituicaoConverter = instituicaoConverter;
    //    this.periodoConverter = periodoConverter;
    //}

    public override Movimentacao Convert(MovimentacaoRequest dto)
    {
        return new Movimentacao
        {
            CategoriaId = dto.CategoriaId,
            PeriodoId = dto.PeriodoId,
            Data = dto.Data,
            Descricao = dto.Descricao,
            InstituicaoId = dto.InstituicaoId,
            Valor = dto.Valor
        };
    }

    public override MovimentacaoRequest Convert(Movimentacao domain)
    {
        return new MovimentacaoRequest
        {
            CategoriaId = domain.CategoriaId,
            PeriodoId = domain.PeriodoId,
            Data = domain.Data,
            Descricao = domain.Descricao,
            InstituicaoId = domain.InstituicaoId,
            Valor = domain.Valor
        };
    }
}
