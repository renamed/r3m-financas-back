using System.Text.Json.Serialization;

namespace R3M.Financas.Back.Domain.Dtos;

public sealed class RespostaAbstrata<T>
{
    [JsonPropertyName("resposta")]
    public T Resposta { get; set; }

    [JsonPropertyName("nome_tipo")]
    public string? NomeTipo { get; set; }

    public static RespostaAbstrata<T> Criar(T resposta, string? nomeTipo = null)
    {
        return new() { NomeTipo = nomeTipo, Resposta = resposta };
    }
}
