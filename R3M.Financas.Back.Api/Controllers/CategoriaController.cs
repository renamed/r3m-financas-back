using Microsoft.AspNetCore.Mvc;
using R3M.Financas.Back.Api.Interfaces;

namespace R3M.Financas.Back.Api.Controllers;

[Route("api/[controller]")]
[ApiController]
public class CategoriaController : ControllerBase
{
    private readonly ICategoriaRepository categoriaRepository;

    public CategoriaController(ICategoriaRepository categoriaRepository)
    {
        this.categoriaRepository = categoriaRepository;
    }

    [HttpGet]
    public async Task<IActionResult> ListAsync([FromQuery] string? nome)
    {
        try
        {
            var categorias =
                string.IsNullOrWhiteSpace(nome)
                    ? await categoriaRepository.ListAsync()
                    : await categoriaRepository.SearchAsync(nome);
            return Ok(categorias);
        }
        catch (Exception ex)
        {
            // Log the exception (not shown here for brevity)
            return StatusCode(500, "Internal server error: " + ex.Message);
        }
    }

    [HttpGet("pai/{parentId:guid?}")]
    public async Task<IActionResult> ListByParentAsync(Guid? parentId)
    {
        var categorias = await categoriaRepository.ListAsync(parentId);
        return Ok(categorias);
    }
}
