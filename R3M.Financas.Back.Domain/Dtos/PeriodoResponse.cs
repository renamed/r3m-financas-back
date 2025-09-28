using System.Text.Json.Serialization;

namespace R3M.Financas.Back.Domain.Dtos;

public class PeriodoResponse
{
    [JsonPropertyName("periodo_id")]
    public Guid PeriodoId { get; set; }

    [JsonPropertyName("nome")]
    public string Nome { get; set; }

    [JsonPropertyName("inicio")]
    public DateOnly Inicio { get; set; }

    [JsonPropertyName("fim")]
    public DateOnly Fim { get; set; }
}

