using Microsoft.EntityFrameworkCore;
using R3M.Financas.Back.Api.Dto;
using R3M.Financas.Back.Api.Interfaces;

namespace R3M.Financas.Back.Api.Data;

public class TipoCategoriaRepository : ITipoCategoriaRepository
{
    private readonly FinancasContext financasContext;

    public TipoCategoriaRepository(FinancasContext financasContext)
    {
        this.financasContext = financasContext;
    }

    public async Task<IReadOnlyList<TipoCategoriaResponse>> ListAsync()
    {
        return await financasContext
            .TipoCategoria
            .Select(s => new TipoCategoriaResponse
            {
                TipoCategoriaId = s.Id,
                Nome = s.Nome
            }).ToListAsync();
    }

    public async Task<TipoCategoriaResponse?> ObterAsync(Guid tipoCategoriaId)
    {
        var res =
            await financasContext
            .TipoCategoria
            .FindAsync(tipoCategoriaId);
        
        if (res == null) return null;

        return new TipoCategoriaResponse
        {
            TipoCategoriaId = res.Id,
            Nome = res.Nome
        };
    }
}
