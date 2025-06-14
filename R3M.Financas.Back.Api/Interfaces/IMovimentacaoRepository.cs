using R3M.Financas.Back.Api.Dto;

namespace R3M.Financas.Back.Api.Interfaces;

public interface IMovimentacaoRepository
{
    Task AdicionarAsync(MovimentacaoRequest movimentacao);
    Task<IReadOnlyList<MovimentacaoResponse>> ListarAsync(Guid instituicaoId);
}
