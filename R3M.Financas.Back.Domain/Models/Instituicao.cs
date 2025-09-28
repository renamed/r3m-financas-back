namespace R3M.Financas.Back.Domain.Models;

public class Instituicao : Registro
{
    public string Nome { get; set; }
    public decimal SaldoInicial { get; set; }
    public decimal SaldoAtual { get; set; }
    public DateOnly DataSaldoInicial { get; set; }
    public bool InstituicaoCredito { get; set; }
    public decimal? LimiteCredito { get; set; }
}
