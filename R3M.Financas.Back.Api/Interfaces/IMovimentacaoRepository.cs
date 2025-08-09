using R3M.Financas.Back.Api.Dto;

namespace R3M.Financas.Back.Api.Interfaces;

public interface IMovimentacaoRepository
{
    Task AdicionarAsync(MovimentacaoRequest movimentacao);
    Task<IReadOnlyList<MovimentacaoResponse>> ListarAsync(Guid instituicaoId, Guid periodoId);
    Task<MovimentacaoResponse?> ObterAsync(Guid id);
    Task DeletarAsync(Guid id);
    Task<int> ContarPorCategoriaAsync(IList<Guid> categoriaId);
}
