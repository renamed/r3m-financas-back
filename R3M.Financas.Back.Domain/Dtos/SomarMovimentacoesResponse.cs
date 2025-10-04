using System.Text.Json.Serialization;

namespace R3M.Financas.Back.Domain.Dtos;

public class SomarMovimentacoesResponse
{
    [JsonPropertyName("categoria")]
    public CategoriaResponse Categoria { get; set; }

    [JsonPropertyName("valor")]
    public decimal Valor { get; set; }
}
