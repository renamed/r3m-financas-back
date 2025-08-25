namespace R3M.Financas.Back.Api.Dto;

public record AplicacoesDtoResponse<T>
(    
    string? Mensagem = null,    
    T? Response = default
)
{
    public bool Sucesso => string.IsNullOrEmpty(Mensagem);
}
