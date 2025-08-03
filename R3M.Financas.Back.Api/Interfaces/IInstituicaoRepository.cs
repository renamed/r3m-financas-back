using R3M.Financas.Back.Api.Dto;

namespace R3M.Financas.Back.Api.Interfaces;

public interface IInstituicaoRepository
{
    Task AtualizarSaldoAsync(Guid id, decimal novoSaldo);
    Task<InstituicaoResponse> CriarAsync(InstituicaoRequest request);
    Task<bool> ExistePorNomeAsync(string nome);
    Task<IReadOnlyList<InstituicaoResponse>> ListarAsync();
    Task<InstituicaoResponse?> ObterAsync(Guid id);
}
