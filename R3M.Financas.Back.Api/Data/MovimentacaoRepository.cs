using Dapper;
using R3M.Financas.Back.Api.Dto;
using R3M.Financas.Back.Api.Interfaces;
using System.Data;

namespace R3M.Financas.Back.Api.Data;

public class MovimentacaoRepository : IMovimentacaoRepository
{
    private readonly IDbConnection dbConnection;

    public MovimentacaoRepository(IDbConnection dbConnection)
    {
        this.dbConnection = dbConnection;
    }

    public async Task<IReadOnlyList<MovimentacaoResponse>> ListarAsync(Guid instituicaoId, Guid periodoId)
    {
        string sql = """
        SELECT
            m.id AS MovimentacaoId,
            m.data AS Data,
            m.descricao AS Descricao,
            m.valor AS Valor,

            i.id AS InstituicaoId,
            i.nome AS Nome,
            i.saldo_atual AS Saldo,
            i.instituicao_credito AS Credito,
            i.limite_credito as LimiteCredito,

            c.id AS CategoriaId,
            c.nome AS Nome,
            c.parent_id AS ParentId,

            p.id AS PeriodoId,
            p.nome AS Nome,
            p.inicio AS Inicio,
            p.fim AS Fim
        FROM
            movimentacoes m
        INNER JOIN
            instituicoes i ON m.instituicao_id = i.id
        INNER JOIN
            categorias c ON m.categoria_id = c.id
        INNER JOIN
            periodos p ON m.periodo_id = p.id
        WHERE
            i.id = @InstituicaoId
            AND p.id = @PeriodoId
    """;

        if (dbConnection.State != ConnectionState.Open)
        {
            dbConnection.Open();
        }

        var result = await dbConnection.QueryAsync<
            MovimentacaoResponse,
            InstituicaoResponse,
            CategoriaResponse,
            PeriodoResponse,
            MovimentacaoResponse>(
            sql,
            (mov, inst, cat, per) =>
            {
                mov.Instituicao = inst;
                mov.Categoria = cat;
                mov.Periodo = per;
                return mov;
            },
            new { InstituicaoId = instituicaoId, PeriodoId = periodoId },
            splitOn: "InstituicaoId,CategoriaId,PeriodoId"
        );

        return [.. result];
    }

    public async Task AdicionarAsync(MovimentacaoRequest movimentacao)
    {
        string sql = "INSERT INTO public.movimentacoes (data, descricao, valor, instituicao_id, periodo_id, categoria_id) " +
            "VALUES(@Data, @Descricao, @Valor, @InstituicaoId, @PeriodoId, @CategoriaId)";

        if (dbConnection.State != ConnectionState.Open)
        {
            dbConnection.Open();
        }
        await dbConnection.ExecuteAsync(sql, movimentacao);
    }
}
