using Microsoft.EntityFrameworkCore;
using R3M.Financas.Back.Domain.Models;
using R3M.Financas.Back.Repository.Contexts;
using R3M.Financas.Back.Repository.Interfaces;
using System.Data;

namespace R3M.Financas.Back.Repository.Data;

public class InstituicaoRepository : IInstituicaoRepository
{
    private readonly FinancasContext financasContext;

    public InstituicaoRepository(FinancasContext financasContext)
    {
        this.financasContext = financasContext;
    }

    public async Task<IReadOnlyList<Instituicao>> ListarAsync()
    {
        return await financasContext.Instituicoes.ToListAsync();
    }

    public async Task<Instituicao?> ObterAsync(Guid id)
    {
        return await financasContext.Instituicoes.FindAsync(id);
    }

    public async Task AtualizarSaldoAsync(Guid id, decimal novoSaldo)
    {
        var instituicao = await financasContext.Instituicoes.FindAsync(id) 
            ?? throw new KeyNotFoundException($"Instituição com ID {id} não encontrada.");

        instituicao.SaldoAtual = novoSaldo;
        financasContext.Instituicoes.Update(instituicao);
        await financasContext.SaveChangesAsync();
    }

    public async Task<Instituicao> CriarAsync(Instituicao instituicao)
    {
        financasContext.Instituicoes.Add(instituicao);
        await financasContext.SaveChangesAsync();
        return instituicao;
    }

    public async Task<bool> ExistePorNomeAsync(string nome)
    {
        return await financasContext
            .Instituicoes
            .AnyAsync(i => EF.Functions.ILike(EF.Functions.Unaccent(i.Nome), EF.Functions.Unaccent($"%{nome}%")));
    }
}
