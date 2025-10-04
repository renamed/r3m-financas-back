using Microsoft.EntityFrameworkCore.Storage;
using R3M.Financas.Back.Domain.Dtos;
using R3M.Financas.Back.Domain.Models;
using System.Data;

namespace R3M.Financas.Back.Repository.Interfaces;

public interface IMovimentacaoRepository
{
    Task AdicionarAsync(Movimentacao movimentacao);
    Task<IReadOnlyList<Movimentacao>> ListarAsync(Guid instituicaoId, Guid periodoId);
    Task<Movimentacao?> ObterAsync(Guid id);
    Task DeletarAsync(Guid id);
    Task<int> ContarPorCategoriaAsync(IList<Guid> categoriaId);
    Task<IDbContextTransaction> IniciarTransacao(IsolationLevel isolationLevel, CancellationToken cs = default);
    Task<IReadOnlyList<SomarMovimentacoesDto>> SomarAsync(Guid periodoId, Guid? categoriaPaiId, Guid? instituicaoId, CancellationToken ct = default);
}
