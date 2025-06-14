namespace R3M.Financas.Back.Api.Dto;

public class MovimentacaoRequest
{
    public DateOnly Data { get; set; }
    public string Descricao { get; set; }
    public decimal Valor { get; set; }
    public Guid CategoriaId { get; set; }
    public Guid PeriodoId { get; set; }
    public Guid InstituicaoId { get; set; }
}
