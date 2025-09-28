using Microsoft.EntityFrameworkCore;
using R3M.Financas.Back.Domain.Models;
using R3M.Financas.Back.Repository.Contexts;
using R3M.Financas.Back.Repository.Interfaces;
using System.Data;

namespace R3M.Financas.Back.Repository.Data;

public class PeriodoRepository : IPeriodoRepository
{
    private readonly FinancasContext financasContext;

    public PeriodoRepository(FinancasContext financasContext)
    {
        this.financasContext = financasContext;
    }

    public async Task<IReadOnlyList<Periodo>> ListarAsync(int anoBase)
    {
        return await financasContext.Periodos.Where(p => p.Inicio.Year == anoBase || p.Fim.Year == anoBase).ToListAsync();
    }

    public async Task<Periodo?> ObterAsync(Guid id)
    {
        return await financasContext.Periodos.FindAsync(id);
    }
}
