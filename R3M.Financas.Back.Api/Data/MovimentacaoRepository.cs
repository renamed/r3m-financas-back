using Microsoft.EntityFrameworkCore;
using R3M.Financas.Back.Api.Dto;
using R3M.Financas.Back.Api.Interfaces;
using R3M.Financas.Back.Api.Modelos;
using System.Data;

namespace R3M.Financas.Back.Api.Data;

public class MovimentacaoRepository : IMovimentacaoRepository
{
    private readonly FinancasContext financasContext;

    public MovimentacaoRepository(FinancasContext financasContext)
    {
        this.financasContext = financasContext;
    }

    public async Task<IReadOnlyList<MovimentacaoResponse>> ListarAsync(Guid instituicaoId, Guid periodoId)
    {
        return await financasContext
            .Movimentacoes
            .Include(i => i.Instituicao)
            .Include(i => i.Categoria)
            .Include(i => i.Periodo)
            .Where(w => w.InstituicaoId == instituicaoId
                    && w.PeriodoId == periodoId)
            .Select(s => new MovimentacaoResponse
            {
                MovimentacaoId = s.Id,
                Data = s.Data,
                Descricao = s.Descricao,
                Valor = s.Valor,
                Categoria = new CategoriaResponse
                {
                    CategoriaId = s.Categoria.Id,
                    Nome = s.Categoria.Nome,
                    ParentId = s.Categoria.ParentId
                },
                Instituicao = new InstituicaoResponse
                {
                    InstituicaoId = s.Instituicao.Id,
                    Nome = s.Instituicao.Nome,
                    Credito = s.Instituicao.InstituicaoCredito,
                    LimiteCredito = s.Instituicao.LimiteCredito,
                    Saldo = s.Instituicao.SaldoAtual
                },
                Periodo = new PeriodoResponse
                {
                    PeriodoId = s.Periodo.Id,
                    Nome = s.Periodo.Nome,
                    Inicio = s.Periodo.Inicio,
                    Fim = s.Periodo.Fim
                }
            }).ToListAsync();
    }

    public async Task AdicionarAsync(MovimentacaoRequest movimentacao)
    {
        var novaMovimentacao = new Movimentacao
        {
            Data = movimentacao.Data,
            Descricao = movimentacao.Descricao,
            Valor = movimentacao.Valor,
            CategoriaId = movimentacao.CategoriaId,
            PeriodoId = movimentacao.PeriodoId,
            InstituicaoId = movimentacao.InstituicaoId
        };

        financasContext.Movimentacoes.Add(novaMovimentacao);
        await financasContext.SaveChangesAsync();
    }

    public async Task<MovimentacaoResponse?> ObterAsync(Guid id)
    {
        var movimentacao = await financasContext
            .Movimentacoes
            .Include(i => i.Instituicao)
            .Include(i => i.Categoria)
            .Include(i => i.Periodo)
            .FirstOrDefaultAsync(w => w.Id == id);

        return movimentacao == null ? null : new MovimentacaoResponse
        {
            MovimentacaoId = movimentacao.Id,
            Data = movimentacao.Data,
            Descricao = movimentacao.Descricao,
            Valor = movimentacao.Valor,
            Categoria = new CategoriaResponse
            {
                CategoriaId = movimentacao.Categoria.Id,
                Nome = movimentacao.Categoria.Nome,
                ParentId = movimentacao.Categoria.ParentId
            },
            Instituicao = new InstituicaoResponse
            {
                InstituicaoId = movimentacao.Instituicao.Id,
                Nome = movimentacao.Instituicao.Nome,
                Credito = movimentacao.Instituicao.InstituicaoCredito,
                LimiteCredito = movimentacao.Instituicao.LimiteCredito,
                Saldo = movimentacao.Instituicao.SaldoAtual
            },
            Periodo = new PeriodoResponse
            {
                PeriodoId = movimentacao.Periodo.Id,
                Nome = movimentacao.Periodo.Nome,
                Inicio = movimentacao.Periodo.Inicio,
                Fim = movimentacao.Periodo.Fim
            }
        };
    }

    public async Task DeletarAsync(Guid id)
    {
        var movimentacao = await financasContext.Movimentacoes.FindAsync(id);
        if (movimentacao == null) return;

        financasContext.Movimentacoes.Remove(movimentacao);
        await financasContext.SaveChangesAsync();
    }
}
