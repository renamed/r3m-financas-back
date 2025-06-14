namespace R3M.Financas.Back.Api.Dto;

public class PeriodoResponse
{
    public Guid PeriodoId { get; set; }
    public string Nome { get; set; }
    public DateOnly Inicio { get; set; }
    public DateOnly Fim { get; set; }
}

