using System.Text.Json.Serialization;

namespace R3M.Financas.Back.Domain.Dtos;

public class TipoCategoriaResponse
{
    [JsonPropertyName("tipo_categoria_id")]
    public Guid TipoCategoriaId { get; set; }

    [JsonPropertyName("nome")]
    public string Nome { get; set; }
}
