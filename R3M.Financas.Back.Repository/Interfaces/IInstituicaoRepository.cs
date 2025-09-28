using R3M.Financas.Back.Domain.Models;

namespace R3M.Financas.Back.Repository.Interfaces;

public interface IInstituicaoRepository
{
    Task AtualizarSaldoAsync(Guid id, decimal novoSaldo);
    Task<Instituicao> CriarAsync(Instituicao request);
    Task<bool> ExistePorNomeAsync(string nome);
    Task<IReadOnlyList<Instituicao>> ListarAsync();
    Task<Instituicao?> ObterAsync(Guid id);
}
