using Microsoft.EntityFrameworkCore;
using R3M.Financas.Back.Domain.Models;
using R3M.Financas.Back.Repository.Contexts;
using R3M.Financas.Back.Repository.Interfaces;

namespace R3M.Financas.Back.Repository.Data;

public class TipoCategoriaRepository : ITipoCategoriaRepository
{
    private readonly FinancasContext financasContext;

    public TipoCategoriaRepository(FinancasContext financasContext)
    {
        this.financasContext = financasContext;
    }

    public async Task<IReadOnlyList<TipoCategoria>> ListAsync()
    {
        return await financasContext
            .TipoCategoria
            .ToListAsync();
    }

    public async Task<TipoCategoria?> ObterAsync(Guid tipoCategoriaId)
    {
        return await financasContext
            .TipoCategoria
            .FindAsync(tipoCategoriaId);
    }
}
