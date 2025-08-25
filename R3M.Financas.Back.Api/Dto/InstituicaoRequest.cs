using System.Text.Json.Serialization;

namespace R3M.Financas.Back.Api.Dto;

public class InstituicaoRequest
{
    [JsonPropertyName("nome")]
    public string Nome { get; set; }
    
    [JsonPropertyName("saldo_inicial")]
    public decimal SaldoInicial { get; set; }
    
    [JsonPropertyName("data_saldo_inicial")]
    public DateOnly DataSaldoInicial { get; set; }

    [JsonPropertyName("instituicao_credito")]
    public bool InstituicaoCredito { get; set; }
    
    [JsonPropertyName("limite_credito")]
    public decimal? LimiteCredito { get; set; }
    
    [JsonPropertyName("dia_fechamento_fatura")]
    public int? DiaFechamentoFatura { get; set; }
}
