using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;
using R3M.Financas.Back.Domain.Dtos;
using R3M.Financas.Back.Domain.Models;
using R3M.Financas.Back.Repository.Contexts;
using R3M.Financas.Back.Repository.Interfaces;
using System.Data;

namespace R3M.Financas.Back.Repository.Data;

public class MovimentacaoRepository : IMovimentacaoRepository
{
    private readonly FinancasContext financasContext;

    public MovimentacaoRepository(FinancasContext financasContext)
    {
        this.financasContext = financasContext;
    }

    public async Task<IDbContextTransaction> IniciarTransacao(IsolationLevel isolationLevel, CancellationToken cs = default)
    {
        return await financasContext.Database.BeginTransactionAsync(isolationLevel, cs);
    }

    public async Task<IReadOnlyList<Movimentacao>> ListarAsync(Guid instituicaoId, Guid periodoId)
    {
        return await financasContext
            .Movimentacoes
            .Include(i => i.Instituicao)
            .Include(i => i.Categoria)
            .Include(i => i.Periodo)
            .Where(w => w.InstituicaoId == instituicaoId
                    && w.PeriodoId == periodoId)
            .ToListAsync();
    }

    public async Task AdicionarAsync(Movimentacao movimentacao)
    {
        financasContext.Movimentacoes.Add(movimentacao);
        await financasContext.SaveChangesAsync();
    }

    public async Task<Movimentacao?> ObterAsync(Guid id)
    {
        return await financasContext
            .Movimentacoes
            .Include(i => i.Instituicao)
            .Include(i => i.Categoria)
            .Include(i => i.Periodo)
            .FirstOrDefaultAsync(w => w.Id == id);
    }

    public async Task<int> ContarPorCategoriaAsync(IList<Guid> categoriaId)
    {

        return await financasContext.Movimentacoes
            .CountAsync(c => categoriaId.Contains(c.CategoriaId));
    }

    public async Task DeletarAsync(Guid id)
    {
        var movimentacao = await financasContext.Movimentacoes.FindAsync(id);
        if (movimentacao == null) return;

        financasContext.Movimentacoes.Remove(movimentacao);
        await financasContext.SaveChangesAsync();
    }

    public async Task<IReadOnlyList<SomarMovimentacoesDto>> SomarAsync(
    Guid periodoId,
    Guid? categoriaPaiId,
    Guid? instituicaoId,
    CancellationToken ct = default)
    {
        var categorias = await financasContext.Categorias
            .Where(cat => cat.ParentId == categoriaPaiId)
            .AsNoTracking()
            .ToListAsync(ct);

        var resultado = new List<SomarMovimentacoesDto>();
        foreach (var categoriaIdAtual in categorias.Select(x => x.Id).Distinct())
        {
            var somaAtual = (await financasContext.Movimentacoes.Where(mov =>
                                mov.PeriodoId == periodoId
                                && (instituicaoId == null || mov.InstituicaoId == instituicaoId)
                                && mov.CategoriaId == categoriaIdAtual)
                            .ToListAsync(ct)).Sum(x => x.Valor);
                
            var somaFilhos = (await SomarAsync(periodoId, categoriaIdAtual, instituicaoId, ct))
                .Sum(x => x.Valor);

            resultado.Add(new SomarMovimentacoesDto
            {
                Id = categoriaIdAtual,
                Nome = categorias.First(x => x.Id == categoriaIdAtual).Nome,
                Valor = somaAtual + somaFilhos
            });
        }

        return resultado;
    }

}
