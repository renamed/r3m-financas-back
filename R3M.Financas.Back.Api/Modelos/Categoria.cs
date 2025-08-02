namespace R3M.Financas.Back.Api.Modelos;

public class Categoria : Registro
{
    public string Nome { get; set; }

    public Categoria? Parent { get; set; }
    public Guid? ParentId { get; set; }
    public List<Categoria> Filhos { get; set; }

    public TipoCategoria? TipoCategoria { get; set; }
    public Guid? TipoCategoriaId { get; set; }
}
