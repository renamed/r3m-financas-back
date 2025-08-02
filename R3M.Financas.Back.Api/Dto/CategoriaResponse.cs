using System.Text.Json.Serialization;

namespace R3M.Financas.Back.Api.Dto;

public class CategoriaResponse
{
    [JsonPropertyName("categoria_id")]
    public Guid CategoriaId { get; set; }

    [JsonPropertyName("nome")]
    public required string Nome { get; set; }

    [JsonPropertyName("parent_id")]
    public Guid? ParentId { get; set; }
}
