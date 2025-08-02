using System.Text.Json.Serialization;

namespace R3M.Financas.Back.Api.Dto;

public class InstituicaoResponse
{
    [JsonPropertyName("instituicao_id")]
    public Guid InstituicaoId { get; set; }

    [JsonPropertyName("nome")]
    public string Nome { get; set; }

    [JsonPropertyName("saldo")]
    public decimal Saldo { get; set; }

    [JsonPropertyName("credito")]
    public bool Credito { get; set; }

    [JsonPropertyName("limite_credito")]
    public decimal? LimiteCredito { get; set; }
}
