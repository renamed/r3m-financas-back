namespace R3M.Financas.Back.Domain.Models;

public class Periodo : Registro
{
    public string Nome { get; set; }

    public DateOnly Inicio { get; set; }

    public DateOnly Fim { get; set; }
}
