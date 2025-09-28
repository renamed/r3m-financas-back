using R3M.Financas.Back.Domain.Models;

namespace R3M.Financas.Back.Repository.Interfaces;

public interface ITipoCategoriaRepository
{
    Task<IReadOnlyList<TipoCategoria>> ListAsync();
    Task<TipoCategoria?> ObterAsync(Guid tipoCategoriaId);
}
