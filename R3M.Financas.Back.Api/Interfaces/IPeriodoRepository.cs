using R3M.Financas.Back.Api.Dto;

namespace R3M.Financas.Back.Api.Interfaces;

public interface IPeriodoRepository
{
    Task<IReadOnlyList<PeriodoResponse>> ListarAsync(int anoBase);
    Task<PeriodoResponse?> ObterAsync(Guid id);
}
