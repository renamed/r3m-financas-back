using R3M.Financas.Back.Api.Dto;
using R3M.Financas.Back.Api.Interfaces;

namespace R3M.Financas.Back.Api.Aplicacoes;

public class InstituicoesBusiness : IInstituicoesBusiness
{
    private readonly IInstituicaoRepository instituicoesRepository;

    public InstituicoesBusiness(IInstituicaoRepository instituicoesRepository)
    {
        this.instituicoesRepository = instituicoesRepository;
    }

    public async Task<AplicacoesDtoResponse<IReadOnlyList<InstituicaoResponse>>> ListarAsync()
    {
        try
        {
            return new AplicacoesDtoResponse<IReadOnlyList<InstituicaoResponse>>(
                null, await instituicoesRepository.ListarAsync());
        }
        catch (Exception ex)
        {
            return ex switch
            {
                ArgumentException => new AplicacoesDtoResponse<IReadOnlyList<InstituicaoResponse>>("Argumento inválido fornecido.", null),
                InvalidOperationException => new AplicacoesDtoResponse<IReadOnlyList<InstituicaoResponse>>("Operação inválida.", null),
                _ => new AplicacoesDtoResponse<IReadOnlyList<InstituicaoResponse>>("Erro ao listar instituições: " + ex.Message, null),
            };
        }
    }
}
