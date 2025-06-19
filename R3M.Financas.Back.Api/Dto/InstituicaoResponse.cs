namespace R3M.Financas.Back.Api.Dto;

public class InstituicaoResponse
{
    public Guid InstituicaoId { get; set; }
    public string Nome { get; set; }
    public decimal Saldo { get; set; }
    public bool Credito { get; set; }
    public decimal? LimiteCredito { get; set; }
}
