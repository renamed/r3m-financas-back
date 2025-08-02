using Microsoft.EntityFrameworkCore;
using R3M.Financas.Back.Api.Dto;
using R3M.Financas.Back.Api.Interfaces;
using System.Data;

namespace R3M.Financas.Back.Api.Data;

public class InstituicaoRepository : IInstituicaoRepository
{
    private readonly FinancasContext financasContext;

    public InstituicaoRepository(FinancasContext financasContext)
    {
        this.financasContext = financasContext;
    }

    public async Task<IReadOnlyList<InstituicaoResponse>> ListarAsync()
    {
        var instituicoes = await financasContext.Instituicoes.ToListAsync();
        return [.. instituicoes.Select(i => new InstituicaoResponse
        {
            InstituicaoId = i.Id,
            Nome = i.Nome,
            Saldo = i.SaldoAtual,
            Credito = i.InstituicaoCredito,
            LimiteCredito = i.LimiteCredito
        })];
    }

    public async Task<InstituicaoResponse?> ObterAsync(Guid id)
    {
        var instituicao = await financasContext.Instituicoes.FindAsync(id);
        
        return instituicao == null ? null : new InstituicaoResponse
        {
            InstituicaoId = instituicao.Id,
            Nome = instituicao.Nome,
            Saldo = instituicao.SaldoAtual,
            Credito = instituicao.InstituicaoCredito,
            LimiteCredito = instituicao.LimiteCredito
        };
    }

    public async Task AtualizarSaldoAsync(Guid id, decimal novoSaldo)
    {
        var instituicao = await financasContext.Instituicoes.FindAsync(id) 
            ?? throw new KeyNotFoundException($"Instituição com ID {id} não encontrada.");

        instituicao.SaldoAtual = novoSaldo;
        financasContext.Instituicoes.Update(instituicao);
        await financasContext.SaveChangesAsync();
    }
}
