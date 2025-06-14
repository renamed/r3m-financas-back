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
}
