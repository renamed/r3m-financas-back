using Microsoft.AspNetCore.Mvc;
using R3M.Financas.Back.Application.Interfaces;
using R3M.Financas.Back.Domain.Dtos;
using R3M.Financas.Back.Domain.Models;
using R3M.Financas.Back.Repository.Interfaces;

namespace R3M.Financas.Back.Api.Controllers;

[Route("api/[controller]")]
[ApiController]
public class CategoriaController : ControllerBase
{
    private readonly ICategoriaRepository categoriaRepository;
    private readonly IMovimentacaoRepository movimentacaoRepository;

    private readonly IConverter<CategoriaResponse, Categoria> converterResponse;
    private readonly IConverter<CategoriaRequest, Categoria> converterRequest;

    public CategoriaController(ICategoriaRepository categoriaRepository, IMovimentacaoRepository movimentacaoRepository, IConverter<CategoriaResponse, Categoria> converterResponse, IConverter<CategoriaRequest, Categoria> converterRequest)
    {
        this.categoriaRepository = categoriaRepository;
        this.movimentacaoRepository = movimentacaoRepository;
        this.converterResponse = converterResponse;
        this.converterRequest = converterRequest;
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
            return Ok(converterResponse.BulkConvert(categorias));
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
        return Ok(converterResponse.BulkConvert(categorias));
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

        var categoria = converterRequest.Convert(request);

        await categoriaRepository.AddAsync(categoria);
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

        var movimentacoesQtd = await movimentacaoRepository.ContarPorCategoriaAsync([.. categoriasFilhas.Select(x => x.Id)]);
        if (movimentacoesQtd != 0)
        {
            return BadRequest("Não é possível excluir uma categoria que possui movimentações associadas.");
        }

        await categoriaRepository.DeleteAsync(categoria.Id);
        return NoContent();
    }
}
