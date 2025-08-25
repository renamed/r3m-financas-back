using Microsoft.AspNetCore.Mvc;
using R3M.Financas.Back.Api.Dto;
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

    [HttpGet("{id}")]
    public async Task<IActionResult> ObterAsync(Guid id)
    {
        var instituicao = await instituicaoRepository.ObterAsync(id);
        if (instituicao == null)
        {
            return NotFound($"Instituição com ID {id} não encontrada.");
        }

        return Ok(instituicao);
    }

    [HttpPost]
    public async Task<IActionResult> CriarAsync([FromBody] InstituicaoRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.Nome)) return BadRequest("O nome da instituição é obrigatório.");
        if (request.Nome.Length < 3 || request.Nome.Length > 25)  return BadRequest("O nome da instituição deve ter entre 3 e 25 caracteres.");
        if (request.DataSaldoInicial == default) return BadRequest("A data do saldo inicial é obrigatória.");
        if (request.InstituicaoCredito)
        {
            if (request.LimiteCredito == 0) return BadRequest("O limite de crédito deve ser informado se a instituição for de crédito.");
            if (request.LimiteCredito < 0) return BadRequest("O limite de crédito não pode ser negativo.");
            if (!request.DiaFechamentoFatura.HasValue) return BadRequest("O dia de fechamento da fatura é obrigatório para uma instituição de crédito.");
            if (request.DiaFechamentoFatura < 1 || request.DiaFechamentoFatura > 31)
            {
                return BadRequest("O dia de fechamento da fatura deve estar entre 1 e 31.");
            }

        } 
        else if (request.LimiteCredito.HasValue && request.LimiteCredito != 0)
        {
            return BadRequest("O limite de crédito não deve ser informado se a instituição não for de crédito.");
        }


        if (await instituicaoRepository.ExistePorNomeAsync(request.Nome))
        {
            return BadRequest($"Já existe uma instituição com o nome '{request.Nome}'.");
        }

        var instituicao = await instituicaoRepository.CriarAsync(request);
        return Created(Request.Path, instituicao);
    }
}
