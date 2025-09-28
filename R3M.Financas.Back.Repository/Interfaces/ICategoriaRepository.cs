using R3M.Financas.Back.Domain.Models;

namespace R3M.Financas.Back.Repository.Interfaces;

public interface ICategoriaRepository
{
    Task AddAsync(Categoria categoria);
    Task DeleteAsync(Guid id);
    Task<IReadOnlyList<Categoria>> ListAllChildrenAsync(Guid parentId);
    Task<IReadOnlyList<Categoria>> ListAsync();
    Task<IReadOnlyList<Categoria>> ListDirectChildrenAsync(Guid? parentId);
    Task<Categoria?> ObterAsync(Guid id);
    Task<IReadOnlyList<Categoria>> SearchAsync(string name);
}
