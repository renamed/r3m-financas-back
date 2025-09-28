using Microsoft.EntityFrameworkCore;
using R3M.Financas.Back.Domain.Models;
using R3M.Financas.Back.Repository.Contexts;
using R3M.Financas.Back.Repository.Interfaces;
using System.Data;

namespace R3M.Financas.Back.Repository.Data;

public class CategoriaRepository : ICategoriaRepository
{
    private readonly FinancasContext financasContext;

    public CategoriaRepository(FinancasContext financasContext)
    {
        this.financasContext = financasContext;
    }

    public async Task<IReadOnlyList<Categoria>> ListAsync()
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

        return await financasContext.Categorias.FromSqlRaw(sql).ToListAsync();
    }

    public async Task<IReadOnlyList<Categoria>> ListDirectChildrenAsync(Guid? parentId)
    {
        return parentId.HasValue
            ? await financasContext.Categorias.Where(c => c.ParentId == parentId.Value).ToListAsync()
            : await financasContext.Categorias.Where(c => c.ParentId == null).ToListAsync();
    }

    public async Task<IReadOnlyList<Categoria>> ListAllChildrenAsync(Guid parentId)
    {
        FormattableString sql = $@"
            WITH RECURSIVE categoria_hierarchy AS (               
                SELECT
                    c.id,
                    c.nome,
                    c.parent_id,
                    1 AS nivel,
                    c.tipo_categoria_id as tipo_categoria_id
                FROM
                    public.categorias c
                WHERE
                    parent_id = {parentId} or id = {parentId}

                UNION ALL

                SELECT
                    c.id,
                    c.nome,
                    c.parent_id,
                    ch.nivel + 1 AS nivel,
                    ch.tipo_categoria_id as tipo_categoria_id
                FROM
                    public.categorias c
                INNER JOIN
                    categoria_hierarchy ch ON c.parent_id = ch.id
            )
            -- Seleciona o resultado final, ordenado pelo caminho
            select distinct
            	id as Id,            	            	
                nome as Nome,
                parent_id as parent_id,
                tipo_categoria_id as tipo_categoria_id
            FROM
                categoria_hierarchy
            ORDER BY
                nome
            ";

        return await financasContext.Categorias.FromSqlInterpolated(sql).ToListAsync();
    }

    public async Task<IReadOnlyList<Categoria>> SearchAsync(string name)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(name, nameof(name));


        return await 
            financasContext.Categorias
                .Where(c => EF.Functions.ILike(
                    EF.Functions.Unaccent(c.Nome),
                    EF.Functions.Unaccent($"%{name}%")
                )).ToListAsync();
    }

    public async Task<Categoria?> ObterAsync(Guid id)
    {
        return await financasContext.Categorias
            .FindAsync(id);
    }

    public async Task AddAsync(Categoria categoria)
    {
        financasContext.Categorias.Add(categoria);
        await financasContext.SaveChangesAsync();
    }

    public async Task DeleteAsync(Guid id)
    {        
        var categoria = await financasContext.Categorias.FindAsync(id)
            ?? throw new KeyNotFoundException("Categoria não encontrada");
        financasContext.Categorias.Remove(categoria);
        await financasContext.SaveChangesAsync();
    }
}
