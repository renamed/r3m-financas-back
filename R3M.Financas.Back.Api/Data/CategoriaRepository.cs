using Dapper;
using R3M.Financas.Back.Api.Dto;
using R3M.Financas.Back.Api.Interfaces;
using System.Data;
using System.Text;

namespace R3M.Financas.Back.Api.Data;

public class CategoriaRepository : ICategoriaRepository
{
    private readonly IDbConnection dbConnection;

    public CategoriaRepository(IDbConnection dbConnection)
    {
        this.dbConnection = dbConnection;
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
                    1 AS nivel
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
                    ch.nivel + 1 AS nivel
                FROM
                    public.categorias c
                INNER JOIN
                    categoria_hierarchy ch ON c.parent_id = ch.id
            )
            -- Seleciona o resultado final, ordenado pelo caminho
            select
            	id as CategoriaId,            	            	
                caminho as Nome
            FROM
                categoria_hierarchy
            ORDER BY
                caminho
            """;

        if (dbConnection.State != ConnectionState.Open)
        {
            dbConnection.Open();
        }
        return [.. await dbConnection
            .QueryAsync<CategoriaResponse>(sql)];
    }

    public async Task<IReadOnlyList<CategoriaResponse>> ListAsync(Guid? parentId)
    {
        var sql = new StringBuilder("select id, nome, parent_id from categorias ");
        if (parentId.HasValue)
        {
            sql.Append("where parent_id = @parentId");
        }
        else
        {
            sql.Append("where parent_id is null");
        }

        if (dbConnection.State != ConnectionState.Open)
        {
            dbConnection.Open();
        }
        return [.. await dbConnection
            .QueryAsync<CategoriaResponse>(sql.ToString(), parentId)];
    }

    public async Task<IReadOnlyList<CategoriaResponse>> SearchAsync(string name)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(name, nameof(name));

        if (dbConnection.State != ConnectionState.Open)
        {
            dbConnection.Open();
        }
        return [.. await dbConnection
            .QueryAsync<CategoriaResponse>("select id, nome, parent_id from categorias where unaccent(nome) ilike unaccent(@name)", new { name = $"%{name}%" })];
    }

    public async Task<CategoriaResponse?> ObterAsync(Guid id)
    {
        var sql = "select id, nome, parent_id from categorias where id = @id";

        if (dbConnection.State != ConnectionState.Open)
        {
            dbConnection.Open();
        }
        return await dbConnection.QueryFirstOrDefaultAsync<CategoriaResponse>(sql, new { id });
    }
}
