using Microsoft.AspNetCore.Mvc;
using R3M.Financas.Back.Api.Interfaces;

namespace R3M.Financas.Back.Api.Controllers;

[Route("api/[controller]")]
[ApiController]
public class TipoCategoriaController : ControllerBase
{
    private readonly ITipoCategoriaRepository tipoCategoriaRepository;
    public TipoCategoriaController(ITipoCategoriaRepository tipoCategoriaRepository)
    {
        this.tipoCategoriaRepository = tipoCategoriaRepository;
    }

    [HttpGet]
    public async Task<IActionResult> ListAsync()
    {
        var tiposCategorias = await tipoCategoriaRepository.ListAsync();
        return Ok(tiposCategorias);
    }

    [HttpGet("{tipoCategoriaId:guid}")]
    public async Task<IActionResult> ObterAsync(Guid tipoCategoriaId)
    {
        var tipoCategoria = await tipoCategoriaRepository.ObterAsync(tipoCategoriaId);
        if (tipoCategoria == null) return NotFound("Tipo de categoria não encontrado");
        
        return Ok(tipoCategoria);
    }
}
