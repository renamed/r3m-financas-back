namespace R3M.Financas.Back.Api.Modelos;

public abstract class Registro
{
    public Guid Id { get; set; }
    public DateTime? Criacao { get; set; }
    public DateTime? Atualizacao { get; set; }
}
