using Dapper;
using R3M.Financas.Back.Api.Dto;
using R3M.Financas.Back.Api.Interfaces;
using System.Data;

namespace R3M.Financas.Back.Api.Data;

public class PeriodoRepository : IPeriodoRepository
{
    private readonly IDbConnection dbConnection;

    public PeriodoRepository(IDbConnection dbConnection)
    {
        this.dbConnection = dbConnection;
    }

    public async Task<IReadOnlyList<PeriodoResponse>> ListarAsync(int anoBase)
    {
        string sql = @"
            SELECT 
                id as PeriodoId, 
                nome as Nome,  
                inicio as Inicio, 
                fim as Fim
            FROM 
                periodos 
            WHERE 
                DATE_PART('year', inicio) = @anoBase
                or DATE_PART('year', fim) = @anoBase";

        if (dbConnection.State != ConnectionState.Open)
        {
            dbConnection.Open();
        }
        return [.. await dbConnection.QueryAsync<PeriodoResponse>(sql, new { anoBase })];
    }

    public async Task<PeriodoResponse?> ObterAsync(Guid id)
    {
        string sql = @"
            SELECT 
                id as PeriodoId, 
                nome as Nome,  
                inicio as Inicio, 
                fim as Fim
            FROM 
                periodos 
            WHERE 
                id = @id";

        if (dbConnection.State != ConnectionState.Open)
        {
            dbConnection.Open();
        }
        return await dbConnection.QueryFirstOrDefaultAsync<PeriodoResponse>(sql, new { id });
    }
}
