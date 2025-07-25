﻿using Microsoft.AspNetCore.Mvc;
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

    [Fact]
    public async Task AdicionarAsync_ShouldReturnCreated_WithNoBody()
    {
        // Arrange
        var request = CriarMovimentacaoRequest();
        var saldoInicial = 200m;
        _mockPeriodoRepo.Setup(r => r.ObterAsync(request.PeriodoId)).ReturnsAsync(new PeriodoResponse());
        _mockInstRepo.Setup(r => r.ObterAsync(request.InstituicaoId)).ReturnsAsync(new InstituicaoResponse
        {
            InstituicaoId = request.InstituicaoId,
            Saldo = saldoInicial,
            Credito = false
        });
        _mockCatRepo.Setup(r => r.ObterAsync(request.CategoriaId)).ReturnsAsync(CriarCategoriaResponse());

        // Act
        var result = await _controller.AdicionarAsync(request);

        // Assert
        var created = Assert.IsType<CreatedResult>(result);
        Assert.Null(created.Value); // Created() sem parâmetros retorna Value nulo
    }

    [Fact]
    public async Task DeletarAsync_ShouldReturnNotFound_WhenMovimentacaoNotFound()
    {
        // Arrange
        var id = Guid.NewGuid();
        _mockMovRepo.Setup(r => r.ObterAsync(id)).ReturnsAsync((MovimentacaoResponse?)null);

        // Act
        var result = await _controller.DeletarAsync(id);

        // Assert
        var notFound = Assert.IsType<NotFoundObjectResult>(result);
        Assert.Equal("movimentacao", notFound.Value);
    }

    [Fact]
    public async Task DeletarAsync_ShouldReturnNoContent_WhenMovimentacaoExists()
    {
        // Arrange
        var id = Guid.NewGuid();
        var instituicaoId = Guid.NewGuid();
        var mov = new MovimentacaoResponse {
            MovimentacaoId = id,
            Valor = 100,
            Instituicao = new InstituicaoResponse { InstituicaoId = instituicaoId }
        };
        var inst = new InstituicaoResponse {
            InstituicaoId = instituicaoId,
            Saldo = 500,
            Credito = false
        };
        _mockMovRepo.Setup(r => r.ObterAsync(id)).ReturnsAsync(mov);
        _mockInstRepo.Setup(r => r.ObterAsync(instituicaoId)).ReturnsAsync(inst);

        // Act
        var result = await _controller.DeletarAsync(id);

        // Assert
        Assert.IsType<OkResult>(result);
        _mockMovRepo.Verify(r => r.DeletarAsync(id), Times.Once);
        _mockInstRepo.Verify(r => r.AtualizarSaldoAsync(instituicaoId, 400), Times.Once);
    }

    [Fact]
    public async Task DeletarAsync_ShouldReturnNotFound_WhenInstituicaoNotFound()
    {
        // Arrange
        var id = Guid.NewGuid();
        var mov = new MovimentacaoResponse
        {
            MovimentacaoId = id,
            Valor = 100,
            Instituicao = new InstituicaoResponse { InstituicaoId = Guid.NewGuid() }
        };
        _mockMovRepo.Setup(r => r.ObterAsync(id)).ReturnsAsync(mov);
        _mockInstRepo.Setup(r => r.ObterAsync(mov.Instituicao.InstituicaoId)).ReturnsAsync((InstituicaoResponse?)null);

        // Act
        var result = await _controller.DeletarAsync(id);

        // Assert
        var notFound = Assert.IsType<NotFoundObjectResult>(result);
        Assert.Equal("instituicao", notFound.Value);
    }

    [Theory]
    [InlineData(true, 100)] // Credito: saldo + valor
    [InlineData(false, -100)] // Debito: saldo - valor
    public async Task DeletarAsync_ShouldUpdateSaldoAndReturnOk_WhenMovimentacaoAndInstituicaoExist(bool isCredito, decimal expectedDelta)
    {
        // Arrange
        var id = Guid.NewGuid();
        var mov = new MovimentacaoResponse
        {
            MovimentacaoId = id,
            Valor = 100,
            Instituicao = new InstituicaoResponse { InstituicaoId = Guid.NewGuid() }
        };
        var saldoInicial = 500m;
        var inst = new InstituicaoResponse
        {
            InstituicaoId = mov.Instituicao.InstituicaoId,
            Saldo = saldoInicial,
            Credito = isCredito
        };
        _mockMovRepo.Setup(r => r.ObterAsync(id)).ReturnsAsync(mov);
        _mockInstRepo.Setup(r => r.ObterAsync(mov.Instituicao.InstituicaoId)).ReturnsAsync(inst);

        // Act
        var result = await _controller.DeletarAsync(id);

        // Assert
        var ok = Assert.IsType<OkResult>(result);
        _mockMovRepo.Verify(r => r.DeletarAsync(id), Times.Once);
        _mockInstRepo.Verify(r => r.AtualizarSaldoAsync(inst.InstituicaoId, saldoInicial + expectedDelta), Times.Once);
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
