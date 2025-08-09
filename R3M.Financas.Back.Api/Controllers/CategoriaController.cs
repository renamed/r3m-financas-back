using Microsoft.AspNetCore.Mvc;
using R3M.Financas.Back.Api.Dto;
using R3M.Financas.Back.Api.Interfaces;

namespace R3M.Financas.Back.Api.Controllers;

[Route("api/[controller]")]
[ApiController]
public class CategoriaController : ControllerBase
{
    private readonly ICategoriaRepository categoriaRepository;
    private readonly IMovimentacaoRepository movimentacaoRepository;

    public CategoriaController(ICategoriaRepository categoriaRepository, IMovimentacaoRepository movimentacaoRepository)
    {
        this.categoriaRepository = categoriaRepository;
        this.movimentacaoRepository = movimentacaoRepository;
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
        var categorias = await categoriaRepository.ListDirectChildrenAsync(parentId);
        return Ok(categorias);
    }

    [HttpPost]
    public async Task<IActionResult> CreateAsync([FromBody] CategoriaRequest request)
    {
        if(string.IsNullOrWhiteSpace(request.Nome)) return BadRequest("Nome é obrigatório");
        if (request.Nome.Length < 3 || request.Nome.Length > 80) return BadRequest("Nome deve ter entre 3 e 80 caracteres");

        if (request.ParentId.HasValue)
        {
            var categoriaPai = await categoriaRepository.ObterAsync(request.ParentId.Value);
            if (categoriaPai == null) return NotFound("Categoria pai não encontrada");
        }

        await categoriaRepository.AddAsync(request);
        return Created();
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteAsync(Guid id)
    {
        var categoria = await categoriaRepository.ObterAsync(id);
        if (categoria == null) return NotFound("Categoria não encontrada");

        var categoriasFilhas = await categoriaRepository.ListAllChildrenAsync(id);
        if (categoriasFilhas.Count > 1)
        {
            return BadRequest("Não é possível excluir uma categoria que possui filhos.");
        }

        var movimentacoesQtd = await movimentacaoRepository.ContarPorCategoriaAsync([.. categoriasFilhas.Select(x => x.CategoriaId)]);
        if (movimentacoesQtd != 0)
        {
            return BadRequest("Não é possível excluir uma categoria que possui movimentações associadas.");
        }

        await categoriaRepository.DeleteAsync(categoria.CategoriaId);
        return NoContent();
    }
}
