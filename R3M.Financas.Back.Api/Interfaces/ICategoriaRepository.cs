using R3M.Financas.Back.Api.Dto;

namespace R3M.Financas.Back.Api.Interfaces;

public interface ICategoriaRepository
{
    Task<IReadOnlyList<CategoriaResponse>> ListAsync();
    Task<IReadOnlyList<CategoriaResponse>> ListAsync(Guid? parentId);
    Task<CategoriaResponse?> ObterAsync(Guid id);
    Task<IReadOnlyList<CategoriaResponse>> SearchAsync(string name);
}
