using Microsoft.AspNetCore.Mvc;
using R3M.Financas.Back.Application.Interfaces;
using R3M.Financas.Back.Domain.Dtos;
using R3M.Financas.Back.Domain.Models;
using R3M.Financas.Back.Repository.Interfaces;

namespace R3M.Financas.Back.Api.Controllers;

[Route("api/[controller]")]
[ApiController]
public class MovimentacaoController : ControllerBase
{
    private readonly IMovimentacaoRepository movimentacaoRepository;
    private readonly IPeriodoRepository periodoRepository;
    private readonly IInstituicaoRepository instituicaoRepository;
    private readonly ICategoriaRepository categoriaRepository;

    private readonly IConverter<MovimentacaoRequest, Movimentacao> converterRequest;
    private readonly IConverter<MovimentacaoResponse, Movimentacao> converterResponse;
    private readonly IConverter<SomarMovimentacoesResponse, SomarMovimentacoesDto> converterSomaResponse;

    public MovimentacaoController(IMovimentacaoRepository movimentacaoRepository, IPeriodoRepository periodoRepository, IInstituicaoRepository instituicaoRepository, ICategoriaRepository categoriaRepository, IConverter<MovimentacaoRequest, Movimentacao> converterRequest, IConverter<MovimentacaoResponse, Movimentacao> converterResponse, IConverter<SomarMovimentacoesResponse, SomarMovimentacoesDto> converterSomaResponse)
    {
        this.movimentacaoRepository = movimentacaoRepository;
        this.periodoRepository = periodoRepository;
        this.instituicaoRepository = instituicaoRepository;
        this.categoriaRepository = categoriaRepository;
        this.converterRequest = converterRequest;
        this.converterResponse = converterResponse;
        this.converterSomaResponse = converterSomaResponse;
    }

    [HttpGet("{instituicaoId:guid}/{periodoId:guid}")]
    public async Task<IActionResult> ListarAsync(Guid instituicaoId, Guid periodoId)
    {
        var movimentacoes = await movimentacaoRepository.ListarAsync(instituicaoId, periodoId);
        return Ok(converterResponse.BulkConvert(movimentacoes));
    }

    [HttpPost]
    public async Task<IActionResult> AdicionarAsync([FromBody] MovimentacaoRequest movimentacaoRequest)
    {
        var periodo = await periodoRepository.ObterAsync(movimentacaoRequest.PeriodoId);
        if (periodo is null) return NotFound(nameof(periodo));

        var instituicao = await instituicaoRepository.ObterAsync(movimentacaoRequest.InstituicaoId);
        if (instituicao is null) return NotFound(nameof(instituicao));

        var categoria = await categoriaRepository.ObterAsync(movimentacaoRequest.CategoriaId);
        if (categoria is null) return NotFound(nameof(categoria));

        var novoSaldo = instituicao.SaldoAtual;
        novoSaldo += instituicao.InstituicaoCredito ? -movimentacaoRequest.Valor : movimentacaoRequest.Valor;

        var movimentacao = converterRequest.Convert(movimentacaoRequest);

        await movimentacaoRepository.AdicionarAsync(movimentacao);
        await instituicaoRepository.AtualizarSaldoAsync(instituicao.Id, novoSaldo);

        return Created();
    }

    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> DeletarAsync(Guid id)
    {
        var movimentacao = await movimentacaoRepository.ObterAsync(id);
        if (movimentacao is null) return NotFound(nameof(movimentacao));

        var instituicao = await instituicaoRepository.ObterAsync(movimentacao.InstituicaoId);
        if (instituicao is null) return NotFound(nameof(instituicao));

        var novoSaldo = instituicao.SaldoAtual;
        novoSaldo += instituicao.InstituicaoCredito ? movimentacao.Valor : -movimentacao.Valor;

        await movimentacaoRepository.DeletarAsync(id);
        await instituicaoRepository.AtualizarSaldoAsync(instituicao.Id, novoSaldo);

        return Ok();
    }

    [HttpGet("periodo/{periodoId}")]
    public async Task<IActionResult> SomarFilhosPorPeriodo(Guid periodoId,
        [FromQuery] Guid? categoriaPaiId,
        [FromQuery] Guid? instituicaoId,
        CancellationToken ct)
    {

        if (categoriaPaiId.HasValue)
        {
            var categoria = await categoriaRepository.ObterAsync(categoriaPaiId.Value);
            if (categoria is null)
            {
                return NotFound(nameof(categoria));
            }
        }

        if (await periodoRepository.ObterAsync(periodoId) is null)
        {
            return NotFound("periodo");
        }

        if (instituicaoId.HasValue)
        {
            var instituicao = await instituicaoRepository.ObterAsync(instituicaoId.Value);
            if (instituicao is null)
            {
                return NotFound(nameof(instituicao));
            }
        }

        var soma = await movimentacaoRepository.SomarAsync(periodoId, categoriaPaiId, instituicaoId, ct);

        return Ok(converterSomaResponse.BulkConvert(soma));
    }
}
