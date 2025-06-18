using Microsoft.AspNetCore.Mvc;
using R3M.Financas.Back.Api.Dto;
using R3M.Financas.Back.Api.Interfaces;

namespace R3M.Financas.Back.Api.Controllers;

[Route("api/[controller]")]
[ApiController]
public class MovimentacaoController : ControllerBase
{
    private readonly IMovimentacaoRepository movimentacaoRepository;
    private readonly IPeriodoRepository periodoRepository;
    private readonly IInstituicaoRepository instituicaoRepository;
    private readonly ICategoriaRepository categoriaRepository;

    public MovimentacaoController(IMovimentacaoRepository movimentacaoRepository, IPeriodoRepository periodoRepository, IInstituicaoRepository instituicaoRepository, ICategoriaRepository categoriaRepository)
    {
        this.movimentacaoRepository = movimentacaoRepository;
        this.periodoRepository = periodoRepository;
        this.instituicaoRepository = instituicaoRepository;
        this.categoriaRepository = categoriaRepository;
    }

    [HttpGet("{instituicaoId:guid}/{periodoId:guid}")]
    public async Task<IActionResult> ListarAsync(Guid instituicaoId, Guid periodoId)
    {
        var movimentacoes = await movimentacaoRepository.ListarAsync(instituicaoId, periodoId);
        return Ok(movimentacoes);
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

        var novoSaldo = instituicao.Saldo;
        novoSaldo += instituicao.Credito ? -movimentacaoRequest.Valor : movimentacaoRequest.Valor;

        await movimentacaoRepository.AdicionarAsync(movimentacaoRequest);
        await instituicaoRepository.AtualizarSaldoAsync(instituicao.InstituicaoId, novoSaldo);

        return Created();
    }

}
