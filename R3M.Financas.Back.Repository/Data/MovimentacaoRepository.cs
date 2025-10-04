using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;
using Npgsql;
using R3M.Financas.Back.Domain.Dtos;
using R3M.Financas.Back.Domain.Models;
using R3M.Financas.Back.Repository.Contexts;
using R3M.Financas.Back.Repository.Interfaces;
using System.Data;

namespace R3M.Financas.Back.Repository.Data;

public class MovimentacaoRepository : IMovimentacaoRepository
{
    private readonly FinancasContext financasContext;

    public MovimentacaoRepository(FinancasContext financasContext)
    {
        this.financasContext = financasContext;
    }

    public async Task<IDbContextTransaction> IniciarTransacao(IsolationLevel isolationLevel, CancellationToken cs = default)
    {
        return await financasContext.Database.BeginTransactionAsync(isolationLevel, cs);
    }

    public async Task<IReadOnlyList<Movimentacao>> ListarAsync(Guid instituicaoId, Guid periodoId)
    {
        return await financasContext
            .Movimentacoes
            .Include(i => i.Instituicao)
            .Include(i => i.Categoria)
            .Include(i => i.Periodo)
            .Where(w => w.InstituicaoId == instituicaoId
                    && w.PeriodoId == periodoId)
            .ToListAsync();
    }

    public async Task AdicionarAsync(Movimentacao movimentacao)
    {
        financasContext.Movimentacoes.Add(movimentacao);
        await financasContext.SaveChangesAsync();
    }

    public async Task<Movimentacao?> ObterAsync(Guid id)
    {
        return await financasContext
            .Movimentacoes
            .Include(i => i.Instituicao)
            .Include(i => i.Categoria)
            .Include(i => i.Periodo)
            .FirstOrDefaultAsync(w => w.Id == id);
    }

    public async Task<int> ContarPorCategoriaAsync(IList<Guid> categoriaId)
    {

        return await financasContext.Movimentacoes
            .CountAsync(c => categoriaId.Contains(c.CategoriaId));
    }

    public async Task DeletarAsync(Guid id)
    {
        var movimentacao = await financasContext.Movimentacoes.FindAsync(id);
        if (movimentacao == null) return;

        financasContext.Movimentacoes.Remove(movimentacao);
        await financasContext.SaveChangesAsync();
    }

    public async Task<IReadOnlyList<SomarMovimentacoesDto>> SomarAsync(Guid periodoId, 
        Guid? categoriaPaiId, 
        Guid? instituicaoId,
        CancellationToken ct = default)
    {
        var query = financasContext.Database.SqlQueryRaw<SomarMovimentacoesDto>("""
          WITH RECURSIVE categoria_hierarchy AS (                
            SELECT
                id,
                nome::text,
                parent_id,
                nome::text AS caminho,
                1 AS nivel
            FROM
                public.categorias
            WHERE
                parent_id = @CategoriaPaiId OR (@CategoriaPaiId IS NULL AND parent_id IS NULL)

            UNION ALL

            SELECT
                c.id,     
                concat(repeat('-', ch.nivel + 1)::text, c.nome)::text,
                --c.nome,
                c.parent_id,
                ch.caminho || ' -> ' || c.nome AS caminho,
                ch.nivel + 1 AS nivel
            FROM
                public.categorias c
            INNER JOIN
                categoria_hierarchy ch ON c.parent_id = ch.id
        ),
        movimentacoes_com_categorias AS (

            select
               	mh.nome as nome,
                mh.caminho,
            	CASE 
                	WHEN m.categoria_id = 'f2cad7ec-1cf7-4c78-9624-10a41a12092a' THEN -m.valor
                	ELSE m.valor
                end as valor,
                mh.id AS categoria_id,
                mh.parent_id as parent_id
            FROM
                public.movimentacoes m
            JOIN
                categoria_hierarchy mh ON m.categoria_id = mh.id
            join
               	PUBLIC.instituicoes i on i.id = m.instituicao_id
            join
               	public.periodos p on p.id = m.periodo_id
            where
               	p.id = @PeriodoId
                and i.id = @InstituicaoId OR (@InstituicaoId IS NULL AND i.id IS NULL)

        ),
        categoria_agregada AS (

            select
           	    ch.nome as nome,
                ch.caminho,
                ch.parent_id,
                SUM(COALESCE(m.valor, 0)) AS total_valor
            FROM
                categoria_hierarchy ch    
            LEFT JOIN
                movimentacoes_com_categorias m ON ch.id = m.categoria_id    
            GROUP BY
                ch.nome, ch.caminho, ch.parent_id
        ),
        categoria_drill_up AS (

            select
            	ch.id as id,
               	ch.nome,
                ch.caminho,
                ch.parent_id,
                ca.total_valor
            FROM
                categoria_agregada ca
            JOIN
                categoria_hierarchy ch ON ca.caminho LIKE ch.caminho || '%'

            UNION ALL


            select
            	ch.id as id,
               	ch.nome, 
                ch.caminho,
                ch.parent_id,
                0 AS total_valor
            FROM
                categoria_hierarchy ch

        )

        select
        	id as Id,
            caminho as Nome,
            SUM(total_valor) AS Valor
        FROM
            categoria_drill_up
        WHERE
        	parent_id = @CategoriaPaiId OR (@CategoriaPaiId IS NULL AND parent_id IS NULL)
        GROUP by 1,2
        having SUM(total_valor) <> 0
        ORDER BY
         caminho asc
        """, 
            new NpgsqlParameter("CategoriaPaiId", categoriaPaiId.HasValue ? categoriaPaiId : DBNull.Value),
            new NpgsqlParameter("PeriodoId", periodoId),
            new NpgsqlParameter("InstituicaoId", instituicaoId.HasValue ? instituicaoId : DBNull.Value)
        );

        return await query.ToListAsync(ct);
    }

    public async Task<List<Guid?>> ListarCategoriasFilhoAsync(Guid? parentId)
    {
        var resposta = new List<Guid?>();

        var filhos = await financasContext
            .Categorias
            .Where(x => x.ParentId == parentId)
            .ToListAsync();

        foreach(var filho in filhos)
        {
            resposta.Add(filho.Id);
            resposta.AddRange(await ListarCategoriasFilhoAsync(filho.Id));
        }

        return resposta;
    }
}
