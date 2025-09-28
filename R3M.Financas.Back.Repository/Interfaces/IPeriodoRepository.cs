using R3M.Financas.Back.Domain.Models;

namespace R3M.Financas.Back.Repository.Interfaces;

public interface IPeriodoRepository
{
    Task<IReadOnlyList<Periodo>> ListarAsync(int anoBase);
    Task<Periodo?> ObterAsync(Guid id);
}
