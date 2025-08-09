using R3M.Financas.Back.Api.Dto;

namespace R3M.Financas.Back.Api.Interfaces;

public interface ITipoCategoriaRepository
{
    Task<IReadOnlyList<TipoCategoriaResponse>> ListAsync();
    Task<TipoCategoriaResponse?> ObterAsync(Guid tipoCategoriaId);
}
