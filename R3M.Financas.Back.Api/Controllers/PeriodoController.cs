using Microsoft.AspNetCore.Mvc;
using Npgsql.Internal;
using R3M.Financas.Back.Application.Interfaces;
using R3M.Financas.Back.Domain.Dtos;
using R3M.Financas.Back.Domain.Models;
using R3M.Financas.Back.Repository.Interfaces;

namespace R3M.Financas.Back.Api.Controllers;

[Route("api/[controller]")]
[ApiController]
public class PeriodoController : ControllerBase
{
    private readonly IPeriodoRepository periodoRepository;
    private readonly IConverter<PeriodoResponse, Periodo> converter;
    public PeriodoController(IPeriodoRepository periodoRepository, IConverter<PeriodoResponse, Periodo> converter)
    {
        this.periodoRepository = periodoRepository;
        this.converter = converter;
    }

    [HttpGet("{anoBase:int}")]
    public async Task<IActionResult> ListarAsync(int anoBase)
    {
        var periodos = await periodoRepository.ListarAsync(anoBase);
        return Ok(converter.BulkConvert(periodos));
    }

    [HttpGet("{id:Guid}")]
    public async Task<IActionResult> ObterAsync(Guid id)
    {
        var periodo = await periodoRepository.ObterAsync(id);
        if (periodo is null)
        {
            return NotFound($"Período com ID {id} não encontrado.");
        }

        return Ok(converter.Convert(periodo));
    }
}
