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
        var listagem = new List<InstituicaoResponse>();
        foreach (var i in instituicoes)
        {
            listagem.Add(new InstituicaoResponse
            {
                InstituicaoId = i.Id,
                Nome = i.Nome,
                Saldo = i.SaldoAtual,
                Credito = i.InstituicaoCredito,
                LimiteCredito = i.LimiteCredito,
                Faturas = await ObterFaturasAsync(i, !i.InstituicaoCredito)
            });
        }

        return listagem;
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
            LimiteCredito = instituicao.LimiteCredito,
            Faturas = await ObterFaturasAsync(instituicao, !instituicao.InstituicaoCredito),
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
            LimiteCredito = request.InstituicaoCredito ? request.LimiteCredito : 0,
            DiaFechamentoFatura = request.InstituicaoCredito ? request.DiaFechamentoFatura : null
        };

        financasContext.Instituicoes.Add(instituicao);
        await financasContext.SaveChangesAsync();

        return new InstituicaoResponse
        {
            InstituicaoId = instituicao.Id,
            Nome = instituicao.Nome,
            Saldo = instituicao.SaldoAtual,            
            Credito = instituicao.InstituicaoCredito,
            LimiteCredito = instituicao.LimiteCredito,
            DiaFechamentoFatura = instituicao.DiaFechamentoFatura,
            Faturas = []
        };
    }

    public async Task<bool> ExistePorNomeAsync(string nome)
    {
        return await financasContext
            .Instituicoes
            .AnyAsync(i => EF.Functions.ILike(EF.Functions.Unaccent(i.Nome), EF.Functions.Unaccent($"%{nome}%")));
    }

    public async Task<List<FaturaResponse>> ObterFaturasAsync(Instituicao instituicao, bool safe)
    {
        List<FaturaResponse> faturas = [];

        if (!instituicao.InstituicaoCredito)
        {
            if (!safe)
            {
                throw new ArgumentException("Só podemos obter faturas de instituições de crédito", nameof(instituicao));
            }

            return [];
        }

        if (!instituicao.DiaFechamentoFatura.HasValue)
        {
            throw new InvalidOperationException("Dia de fechamento da fatura não definido para instituição de crédito.");
        }

        var movimentacoes = await financasContext
            .Movimentacoes
            .Include(x => x.Periodo)
            .Where(m => m.InstituicaoId == instituicao.Id
                && m.Periodo.Inicio.Month >= DateTime.UtcNow.Month
                && m.Periodo.Inicio.Year >= DateTime.UtcNow.Year)
            .ToListAsync();

        var periodos = movimentacoes
            .Select(m => m.Periodo)
            .Distinct()
            .ToList();

        var diaFechamento = instituicao.DiaFechamentoFatura.Value;
        var datas = new HashSet<ObterFaturasAux>();
        foreach (var periodo in periodos)
        {
            int diasMes = DateTime.DaysInMonth(periodo.Fim.Year, periodo.Fim.Month);
            if (diaFechamento == 1)
            {
                datas.Add(new ObterFaturasAux
                {
                    DataInicio = new DateOnly(periodo.Fim.Year, periodo.Fim.Month, 1),
                    DataFim = new DateOnly(periodo.Fim.Year, periodo.Fim.Month, 1),
                    Periodo = periodo
                });

                datas.Add(new ObterFaturasAux
                {
                    DataInicio = new DateOnly(periodo.Fim.Year, periodo.Fim.Month, 2),
                    DataFim = new DateOnly(periodo.Fim.Year, periodo.Fim.Month, diasMes),
                    Periodo = periodo
                });
            }
            else if (diaFechamento == diasMes)
            {
                datas.Add(new ObterFaturasAux
                {
                    DataInicio = new DateOnly(periodo.Fim.Year, periodo.Fim.Month, 1),
                    DataFim = new DateOnly(periodo.Fim.Year, periodo.Fim.Month, diaFechamento),
                    Periodo = periodo
                });
            }
            else
            {
                datas.Add(new ObterFaturasAux
                {
                    DataInicio = new DateOnly(periodo.Inicio.Year, periodo.Inicio.Month, 1),
                    DataFim = new DateOnly(periodo.Fim.Year, periodo.Fim.Month, diaFechamento),
                    Periodo = periodo
                });

                datas.Add(new ObterFaturasAux
                {
                    DataInicio = new DateOnly(periodo.Inicio.Year, periodo.Inicio.Month, diaFechamento + 1),
                    DataFim = new DateOnly(periodo.Fim.Year, periodo.Fim.Month, diasMes),
                    Periodo = periodo
                });
            }
        }

        foreach (var data in datas)
        {
            faturas.Add(new FaturaResponse
            {
                Periodo = new PeriodoResponse
                {
                    PeriodoId = data.Periodo.Id,
                    Nome = data.Periodo.Nome,
                    Inicio = data.Periodo.Inicio,
                    Fim = data.Periodo.Fim
                },
                Valor = movimentacoes
                            .Where(p => p.Data >= data.DataInicio
                                        && p.Data <= data.DataFim)
                            .Sum(s => s.Valor)
            });
        }
        return faturas;
    }
}

class ObterFaturasAux
{
    public DateOnly DataInicio { get; set; }
    public DateOnly DataFim { get; set; }
    public Periodo Periodo { get; set; }
}