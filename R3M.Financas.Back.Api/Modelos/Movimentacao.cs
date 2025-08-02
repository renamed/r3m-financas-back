namespace R3M.Financas.Back.Api.Modelos;

public class Movimentacao : Registro
{
    public DateOnly Data { get; set; }
    public string Descricao { get; set; }
    public decimal Valor { get; set; }

    public Instituicao Instituicao { get; set; }
    public Guid InstituicaoId { get; set; }

    public Categoria Categoria { get; set; }
    public Guid CategoriaId { get; set; }

    public Periodo Periodo { get; set; }
    public Guid PeriodoId { get; set; }
}
