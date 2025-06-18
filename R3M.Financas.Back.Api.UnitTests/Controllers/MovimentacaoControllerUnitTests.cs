using Microsoft.AspNetCore.Mvc;
using Moq;
using R3M.Financas.Back.Api.Controllers;
using R3M.Financas.Back.Api.Dto;
using R3M.Financas.Back.Api.Interfaces;

namespace R3M.Financas.Back.Api.UnitTests.Controllers;

public class MovimentacaoControllerUnitTests
{
    private readonly Mock<IMovimentacaoRepository> _mockMovRepo;
    private readonly Mock<IPeriodoRepository> _mockPeriodoRepo;
    private readonly Mock<IInstituicaoRepository> _mockInstRepo;
    private readonly Mock<ICategoriaRepository> _mockCatRepo;
    private readonly MovimentacaoController _controller;

    public MovimentacaoControllerUnitTests()
    {
        _mockMovRepo = new Mock<IMovimentacaoRepository>();
        _mockPeriodoRepo = new Mock<IPeriodoRepository>();
        _mockInstRepo = new Mock<IInstituicaoRepository>();
        _mockCatRepo = new Mock<ICategoriaRepository>();

        _controller = new MovimentacaoController(
            _mockMovRepo.Object,
            _mockPeriodoRepo.Object,
            _mockInstRepo.Object,
            _mockCatRepo.Object
        );
    }

    [Fact]
    public async Task ListarAsync_ShouldReturnMovimentacoes()
    {
        // Arrange
        var instituicaoId = Guid.NewGuid();
        var periodoId = Guid.NewGuid();
        var expected = new List<MovimentacaoResponse>
            {
                new MovimentacaoResponse
                {
                    MovimentacaoId = Guid.NewGuid(),
                    Data = new DateOnly(2025, 6, 1),
                    Descricao = "Compra",
                    Valor = 100,
                    Instituicao = new InstituicaoResponse(),
                    Categoria = CriarCategoriaResponse(),
                    Periodo = new PeriodoResponse()
                }
            };

        _mockMovRepo.Setup(repo => repo.ListarAsync(instituicaoId, periodoId)).ReturnsAsync(expected);

        // Act
        var result = await _controller.ListarAsync(instituicaoId, periodoId);

        // Assert
        var ok = Assert.IsType<OkObjectResult>(result);
        Assert.Equal(expected, ok.Value);
    }

    [Fact]
    public async Task AdicionarAsync_ShouldReturnNotFound_WhenPeriodoNotFound()
    {
        // Arrange
        var request = CriarMovimentacaoRequest();
        _mockPeriodoRepo.Setup(r => r.ObterAsync(request.PeriodoId)).ReturnsAsync((PeriodoResponse?)null);

        // Act
        var result = await _controller.AdicionarAsync(request);

        // Assert
        var notFound = Assert.IsType<NotFoundObjectResult>(result);
        Assert.Equal("periodo", notFound.Value);
    }

    [Fact]
    public async Task AdicionarAsync_ShouldReturnNotFound_WhenInstituicaoNotFound()
    {
        // Arrange
        var request = CriarMovimentacaoRequest();
        _mockPeriodoRepo.Setup(r => r.ObterAsync(request.PeriodoId)).ReturnsAsync(new PeriodoResponse());
        _mockInstRepo.Setup(r => r.ObterAsync(request.InstituicaoId)).ReturnsAsync((InstituicaoResponse?)null);

        // Act
        var result = await _controller.AdicionarAsync(request);

        // Assert
        var notFound = Assert.IsType<NotFoundObjectResult>(result);
        Assert.Equal("instituicao", notFound.Value);
    }

    [Fact]
    public async Task AdicionarAsync_ShouldReturnNotFound_WhenCategoriaNotFound()
    {
        // Arrange
        var request = CriarMovimentacaoRequest();
        _mockPeriodoRepo.Setup(r => r.ObterAsync(request.PeriodoId)).ReturnsAsync(new PeriodoResponse());
        _mockInstRepo.Setup(r => r.ObterAsync(request.InstituicaoId)).ReturnsAsync(new InstituicaoResponse());
        _mockCatRepo.Setup(r => r.ObterAsync(request.CategoriaId)).ReturnsAsync((CategoriaResponse?)null);

        // Act
        var result = await _controller.AdicionarAsync(request);

        // Assert
        var notFound = Assert.IsType<NotFoundObjectResult>(result);
        Assert.Equal("categoria", notFound.Value);
    }

    [Theory]
    [InlineData(true, -100)] // Credito
    [InlineData(false, 100)] // Debito
    public async Task AdicionarAsync_ShouldAddAndUpdateSaldo_WhenValidRequest(bool isCredito, decimal expectedDelta)
    {
        // Arrange
        var request = CriarMovimentacaoRequest();
        var saldoInicial = 500m;

        _mockPeriodoRepo.Setup(r => r.ObterAsync(request.PeriodoId)).ReturnsAsync(new PeriodoResponse());
        _mockInstRepo.Setup(r => r.ObterAsync(request.InstituicaoId)).ReturnsAsync(new InstituicaoResponse
        {
            InstituicaoId = request.InstituicaoId,
            Saldo = saldoInicial,
            Credito = isCredito
        });
        _mockCatRepo.Setup(r => r.ObterAsync(request.CategoriaId)).ReturnsAsync(CriarCategoriaResponse());

        // Act
        var result = await _controller.AdicionarAsync(request);

        // Assert
        Assert.IsType<CreatedResult>(result);
        _mockMovRepo.Verify(r => r.AdicionarAsync(request), Times.Once);
        _mockInstRepo.Verify(r => r.AtualizarSaldoAsync(request.InstituicaoId, saldoInicial + expectedDelta), Times.Once);
    }

    private MovimentacaoRequest CriarMovimentacaoRequest()
    {
        return new MovimentacaoRequest
        {
            PeriodoId = Guid.NewGuid(),
            InstituicaoId = Guid.NewGuid(),
            CategoriaId = Guid.NewGuid(),
            Valor = 100,
            Data = new DateOnly(2025, 6, 14),
            Descricao = "Teste"
        };
    }

    private CategoriaResponse CriarCategoriaResponse()
    {
        return new CategoriaResponse
        {
            CategoriaId = Guid.NewGuid(),
            Nome = "Alimentos",
            ParentId = null
        };
    }
}
