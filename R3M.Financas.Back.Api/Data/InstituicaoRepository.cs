using Microsoft.EntityFrameworkCore;
using R3M.Financas.Back.Api.Dto;
using R3M.Financas.Back.Api.Interfaces;
using R3M.Financas.Back.Api.Modelos;
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

    public async Task<InstituicaoResponse> CriarAsync(InstituicaoRequest request)
    {
        var instituicao = new Instituicao
        {
            Nome = request.Nome,
            SaldoInicial = request.SaldoInicial,
            DataSaldoInicial = request.DataSaldoInicial,
            SaldoAtual = request.SaldoInicial,
            InstituicaoCredito = request.InstituicaoCredito,
            LimiteCredito = request.InstituicaoCredito ? request.LimiteCredito : 0
        };

        financasContext.Instituicoes.Add(instituicao);
        await financasContext.SaveChangesAsync();

        return new InstituicaoResponse
        {
            InstituicaoId = instituicao.Id,
            Nome = instituicao.Nome,
            Saldo = instituicao.SaldoAtual,            
            Credito = instituicao.InstituicaoCredito,
            LimiteCredito = instituicao.LimiteCredito
        };
    }

    public async Task<bool> ExistePorNomeAsync(string nome)
    {
        return await financasContext
            .Instituicoes
            .AnyAsync(i => EF.Functions.ILike(EF.Functions.Unaccent(i.Nome), EF.Functions.Unaccent($"%{nome}%")));
    }
}
