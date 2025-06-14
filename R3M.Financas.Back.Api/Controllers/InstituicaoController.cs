using Microsoft.AspNetCore.Mvc;
using R3M.Financas.Back.Api.Interfaces;

namespace R3M.Financas.Back.Api.Controllers;

[Route("api/[controller]")]
[ApiController]
public class InstituicaoController : ControllerBase
{
    private readonly IInstituicaoRepository instituicaoRepository;
    public InstituicaoController(IInstituicaoRepository instituicaoRepository)
    {
        this.instituicaoRepository = instituicaoRepository;
    }

    [HttpGet]
    public async Task<IActionResult> ListarAsync()
    {
        var instituicoes = await instituicaoRepository.ListarAsync();
        return Ok(instituicoes);
    }
}
