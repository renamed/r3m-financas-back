using System.Text.Json.Serialization;

namespace R3M.Financas.Back.Api.Dto;

public class CategoriaRequest
{
    [JsonPropertyName("nome")]
    public string Nome { get; set; }
    
    [JsonPropertyName("parent_id")]
    public Guid? ParentId { get; set; }

    [JsonPropertyName("tipo_categoria_id")]
    public Guid? TipoCategoriaId { get; set; }
}
