using Dapper;
using R3M.Financas.Back.Api.Dto;
using R3M.Financas.Back.Api.Interfaces;
using System.Data;

namespace R3M.Financas.Back.Api.Data;

public class InstituicaoRepository : IInstituicaoRepository
{
    private readonly IDbConnection dbConnection;

    public InstituicaoRepository(IDbConnection dbConnection)
    {
        this.dbConnection = dbConnection;
    }

    public async Task<IReadOnlyList<InstituicaoResponse>> ListarAsync()
    {
        const string sql = @"
            SELECT 
                id as InstituicaoId, 
                nome as Nome, 
                saldo_atual as Saldo, 
                instituicao_credito as Credito
            FROM 
                instituicoes";
        if (dbConnection.State != ConnectionState.Open)
        {
            dbConnection.Open();
        }
        return [.. await dbConnection.QueryAsync<InstituicaoResponse>(sql)];
    }

    public async Task<InstituicaoResponse?> ObterAsync(Guid id)
    {
        const string sql = @"
            SELECT 
                id as InstituicaoId, 
                nome as Nome, 
                saldo_atual as Saldo, 
                instituicao_credito as Credito
            FROM 
                instituicoes
            WHERE
                id = @id";
        if (dbConnection.State != ConnectionState.Open)
        {
            dbConnection.Open();
        }
        return await dbConnection.QueryFirstOrDefaultAsync<InstituicaoResponse>(sql, new { id });
    }

    public async Task AtualizarSaldoAsync(Guid id, decimal novoSaldo)
    {
        const string sql = @"
            UPDATE
                instituicoes
            SET
                saldo_atual = @novoSaldo
            WHERE
                id = @id";

        if (dbConnection.State != ConnectionState.Open)
        {
            dbConnection.Open();
        }
        await dbConnection.ExecuteAsync(sql, new { id, novoSaldo });
    }
}
