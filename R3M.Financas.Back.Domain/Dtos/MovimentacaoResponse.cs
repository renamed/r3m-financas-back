using System.Text.Json.Serialization;

namespace R3M.Financas.Back.Domain.Dtos;

public class MovimentacaoResponse
{
    [JsonPropertyName("movimentacao_id")]
    public Guid MovimentacaoId { get; set; }

    [JsonPropertyName("data")]
    public DateOnly Data { get; set; }

    [JsonPropertyName("descricao")]
    public string Descricao { get; set; }

    [JsonPropertyName("valor")]
    public decimal Valor { get; set; }

    [JsonPropertyName("instituicao")]
    public InstituicaoResponse Instituicao { get; set; }

    [JsonPropertyName("categoria")]
    public CategoriaResponse Categoria { get; set; }

    [JsonPropertyName("periodo")]
    public PeriodoResponse Periodo { get; set; }
}
