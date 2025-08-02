using Microsoft.EntityFrameworkCore;
using R3M.Financas.Back.Api.Dto;
using R3M.Financas.Back.Api.Interfaces;
using System.Data;

namespace R3M.Financas.Back.Api.Data;

public class PeriodoRepository : IPeriodoRepository
{
    private readonly FinancasContext financasContext;

    public PeriodoRepository(FinancasContext financasContext)
    {
        this.financasContext = financasContext;
    }

    public async Task<IReadOnlyList<PeriodoResponse>> ListarAsync(int anoBase)
    {
        var periodos = await financasContext.Periodos.Where(p => p.Inicio.Year == anoBase || p.Fim.Year == anoBase).ToListAsync();
        return [.. periodos.Select(p => new PeriodoResponse
        {
            PeriodoId = p.Id,
            Nome = p.Nome,
            Inicio = p.Inicio,
            Fim = p.Fim
        })];

    }

    public async Task<PeriodoResponse?> ObterAsync(Guid id)
    {
        var periodo = await financasContext.Periodos.FindAsync(id);
        return periodo == null ? null : new PeriodoResponse
        {
            PeriodoId = periodo.Id,
            Nome = periodo.Nome,
            Inicio = periodo.Inicio,
            Fim = periodo.Fim
        };
    }
}
