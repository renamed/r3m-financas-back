namespace R3M.Financas.Back.Api.Dto;

public class CategoriaResponse
{
    public Guid CategoriaId { get; set; }
    public required string Nome { get; set; }
    public Guid? ParentId { get; set; }
}
