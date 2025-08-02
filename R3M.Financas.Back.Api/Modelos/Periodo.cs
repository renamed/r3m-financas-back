using System;
using System.Collections.Generic;

namespace R3M.Financas.Back.Api.Modelos;

public class Periodo : Registro
{
    public string Nome { get; set; }

    public DateOnly Inicio { get; set; }

    public DateOnly Fim { get; set; }
}
