namespace R3M.Financas.Back.Api.Dto;

public class MovimentacaoResponse
{
    public Guid MovimentacaoId { get; set; }
    public DateOnly Data { get; set; }
    public string Descricao { get; set; }
    public decimal Valor { get; set; }

    public InstituicaoResponse Instituicao { get; set; }
    public CategoriaResponse Categoria { get; set; }
    public PeriodoResponse Periodo { get; set; }
}
