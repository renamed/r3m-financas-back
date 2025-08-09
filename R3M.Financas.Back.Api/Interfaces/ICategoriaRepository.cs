using R3M.Financas.Back.Api.Dto;

namespace R3M.Financas.Back.Api.Interfaces;

public interface ICategoriaRepository
{
    Task AddAsync(CategoriaRequest categoria);
    Task DeleteAsync(Guid id);
    Task<IReadOnlyList<CategoriaResponse>> ListAllChildrenAsync(Guid parentId);
    Task<IReadOnlyList<CategoriaResponse>> ListAsync();
    Task<IReadOnlyList<CategoriaResponse>> ListDirectChildrenAsync(Guid? parentId);
    Task<CategoriaResponse?> ObterAsync(Guid id);
    Task<IReadOnlyList<CategoriaResponse>> SearchAsync(string name);
}
