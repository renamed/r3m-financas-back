namespace R3M.Financas.Back.Domain.Models;

public abstract class Registro
{
    public Guid Id { get; set; }
    public DateTime? Criacao { get; set; }
    public DateTime? Atualizacao { get; set; }
}
