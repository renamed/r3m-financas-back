using System.Text.Json.Serialization;

namespace R3M.Financas.Back.Domain.Dtos;

public class MovimentacaoRequest
{
    public DateOnly Data { get; set; }
    public string Descricao { get; set; }
    public decimal Valor { get; set; }
    [JsonPropertyName("categoria_id")]
    public Guid CategoriaId { get; set; }
    [JsonPropertyName("periodo_id")]
    public Guid PeriodoId { get; set; }
    [JsonPropertyName("instituicao_id")]
    public Guid InstituicaoId { get; set; }
}
