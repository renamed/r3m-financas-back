
using System.Text.Json.Serialization;

namespace R3M.Financas.Back.Api.Dto;

public class TipoCategoriaResponse
{
    [JsonPropertyName("tipo_categoria_id")]
    public Guid TipoCategoriaId { get; set; }

    [JsonPropertyName("nome")]
    public string Nome { get; set; }
}
