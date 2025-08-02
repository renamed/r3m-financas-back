using Microsoft.EntityFrameworkCore;
using R3M.Financas.Back.Api.Dto;
using R3M.Financas.Back.Api.Interfaces;
using System.Data;

namespace R3M.Financas.Back.Api.Data;

public class CategoriaRepository : ICategoriaRepository
{
    private readonly FinancasContext financasContext;

    public CategoriaRepository(FinancasContext financasContext)
    {
        this.financasContext = financasContext;
    }

    public async Task<IReadOnlyList<CategoriaResponse>> ListAsync()
    {
        string sql = """
               WITH RECURSIVE categoria_hierarchy AS (               
                SELECT
                    c.id,
                    c.nome,
                    c.parent_id,
                    c.nome::text AS caminho,
                    1 AS nivel,
                    c.tipo_categoria_id as tipo_categoria_id
                FROM
                    public.categorias c
                WHERE
                    parent_id IS null 

                UNION ALL
                
                SELECT
                    c.id,
                    c.nome,
                    c.parent_id,
                    ch.caminho || ' -> ' || c.nome AS caminho,
                    ch.nivel + 1 AS nivel,
                    ch.tipo_categoria_id as tipo_categoria_id
                FROM
                    public.categorias c
                INNER JOIN
                    categoria_hierarchy ch ON c.parent_id = ch.id
            )
            -- Seleciona o resultado final, ordenado pelo caminho
            select
            	id as Id,            	            	
                caminho as Nome,
                parent_id as parent_id,
                tipo_categoria_id as tipo_categoria_id            
            FROM
                categoria_hierarchy
            ORDER BY
                caminho
            """;

        var categorias = await financasContext.Categorias.FromSqlRaw(sql).ToListAsync();
        return [.. categorias.Select(c => new CategoriaResponse
        {
            CategoriaId = c.Id,
            Nome = c.Nome,
            ParentId = c.ParentId
        })];
    }

    public async Task<IReadOnlyList<CategoriaResponse>> ListAsync(Guid? parentId)
    {
        var categorias = parentId.HasValue
            ? financasContext.Categorias.Where(c => c.ParentId == parentId.Value)
            : financasContext.Categorias.Where(c => c.ParentId == null);

        return [.. await categorias
            .Select(c => new CategoriaResponse
            {
                CategoriaId = c.Id,
                Nome = c.Nome,
                ParentId = c.ParentId
            })
            .ToListAsync()];
    }

    public async Task<IReadOnlyList<CategoriaResponse>> SearchAsync(string name)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(name, nameof(name));


        return await 
            financasContext.Categorias
                .Where(c => EF.Functions.ILike(
                    EF.Functions.Unaccent(c.Nome),
                    EF.Functions.Unaccent($"%{name}%")
                ))
                .Select(c => new CategoriaResponse
                {
                    CategoriaId = c.Id,
                    Nome = c.Nome,
                    ParentId = c.ParentId
                }).ToListAsync();
    }

    public async Task<CategoriaResponse?> ObterAsync(Guid id)
    {
        var categoria = await financasContext.Categorias
            .FindAsync(id);

        return categoria == null ? null : new CategoriaResponse
        {
            CategoriaId = categoria.Id,
            Nome = categoria.Nome,
            ParentId = categoria.ParentId
        };
    }
}
