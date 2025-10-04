using R3M.Financas.Back.Domain.Models;

namespace R3M.Financas.Back.Domain.Dtos;

public class SomarMovimentacoesDto
{
    public Guid Id { get; set; }
    public string Nome { get; set; }
    public decimal Valor { get; set; }
}
