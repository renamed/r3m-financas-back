using Microsoft.AspNetCore.Mvc;
using R3M.Financas.Back.Api.Interfaces;

namespace R3M.Financas.Back.Api.Controllers;

[Route("api/[controller]")]
[ApiController]
public class PeriodoController : ControllerBase
{
    private readonly IPeriodoRepository periodoRepository;
    public PeriodoController(IPeriodoRepository periodoRepository)
    {
        this.periodoRepository = periodoRepository;
    }

    [HttpGet("{anoBase:int}")]
    public async Task<IActionResult> ListarAsync(int anoBase)
    {
        var periodos = await periodoRepository.ListarAsync(anoBase);
        return Ok(periodos);
    }

    [HttpGet("{id:Guid}")]
    public async Task<IActionResult> ObterAsync(Guid id)
    {
        var periodo = await periodoRepository.ObterAsync(id);
        if (periodo is null)
        {
            return NotFound($"Período com ID {id} não encontrado.");
        }

        return Ok(periodo);
    }
}
