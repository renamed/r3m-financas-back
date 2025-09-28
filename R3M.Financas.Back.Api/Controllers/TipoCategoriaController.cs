using Microsoft.AspNetCore.Mvc;
using R3M.Financas.Back.Application.Interfaces;
using R3M.Financas.Back.Domain.Dtos;
using R3M.Financas.Back.Domain.Models;
using R3M.Financas.Back.Repository.Interfaces;

namespace R3M.Financas.Back.Api.Controllers;

[Route("api/[controller]")]
[ApiController]
public class TipoCategoriaController : ControllerBase
{
    private readonly ITipoCategoriaRepository tipoCategoriaRepository;

    private readonly IConverter<TipoCategoriaResponse, TipoCategoria> converter;

    public TipoCategoriaController(ITipoCategoriaRepository tipoCategoriaRepository, IConverter<TipoCategoriaResponse, TipoCategoria> converter)
    {
        this.tipoCategoriaRepository = tipoCategoriaRepository;
        this.converter = converter;
    }

    [HttpGet]
    public async Task<IActionResult> ListAsync()
    {
        var tiposCategorias = await tipoCategoriaRepository.ListAsync();
        return Ok(converter.BulkConvert(tiposCategorias));
    }

    [HttpGet("{tipoCategoriaId:guid}")]
    public async Task<IActionResult> ObterAsync(Guid tipoCategoriaId)
    {
        var tipoCategoria = await tipoCategoriaRepository.ObterAsync(tipoCategoriaId);
        if (tipoCategoria == null) return NotFound("Tipo de categoria não encontrado");
        
        return Ok(converter.Convert(tipoCategoria));
    }
}
