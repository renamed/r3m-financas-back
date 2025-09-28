using Microsoft.AspNetCore.Mvc;
using Moq;
using R3M.Financas.Back.Api.Controllers;
using R3M.Financas.Back.Application.Interfaces;
using R3M.Financas.Back.Domain.Dtos;
using R3M.Financas.Back.Domain.Models;
using R3M.Financas.Back.Repository.Interfaces;

namespace R3M.Financas.Back.Api.UnitTests.Controllers;

public class MovimentacaoControllerUnitTests
{
    private readonly Mock<IMovimentacaoRepository> _mockMovRepo;
    private readonly Mock<IPeriodoRepository> _mockPeriodoRepo;
    private readonly Mock<IInstituicaoRepository> _mockInstRepo;
    private readonly Mock<ICategoriaRepository> _mockCatRepo;
    private readonly Mock<IConverter<MovimentacaoRequest, Movimentacao>> _mockConverterRequest;
    private readonly Mock<IConverter<MovimentacaoResponse, Movimentacao>> _mockConverterResponse;
    private readonly MovimentacaoController _controller;

    public MovimentacaoControllerUnitTests()
    {
        _mockMovRepo = new Mock<IMovimentacaoRepository>();
        _mockPeriodoRepo = new Mock<IPeriodoRepository>();
        _mockInstRepo = new Mock<IInstituicaoRepository>();
        _mockCatRepo = new Mock<ICategoriaRepository>();
        _mockConverterRequest = new Mock<IConverter<MovimentacaoRequest, Movimentacao>>();
        _mockConverterResponse = new Mock<IConverter<MovimentacaoResponse, Movimentacao>>();

        _controller = new MovimentacaoController(
            _mockMovRepo.Object,
            _mockPeriodoRepo.Object,
            _mockInstRepo.Object,
            _mockCatRepo.Object,
            _mockConverterRequest.Object,
            _mockConverterResponse.Object
        );
    }

    [Fact]
    public async Task ListarAsync_ShouldReturnMovimentacoes()
    {
        // Arrange
        var instituicaoId = Guid.NewGuid();
        var periodoId = Guid.NewGuid();
        var expected = new List<Movimentacao>
            {
                new Movimentacao
                {
                    Id = Guid.NewGuid(),
                    Data = new DateOnly(2025, 6, 1),
                    Descricao = "Compra",
                    Valor = 100,
                    Instituicao = new Instituicao(),
                    Categoria = CriarCategoriaResponse(),
                    Periodo = new Periodo()
                }
            };

        _mockConverterResponse.Setup(conv => conv.BulkConvert(expected))
            .Returns(expected.Select(m => new MovimentacaoResponse
            {
                MovimentacaoId = m.Id,
                Data = m.Data,
                Descricao = m.Descricao,
                Valor = m.Valor,
                Instituicao = new InstituicaoResponse { InstituicaoId = m.InstituicaoId },
                Categoria = new CategoriaResponse { CategoriaId = m.CategoriaId, Nome="" },
                Periodo = new PeriodoResponse() { PeriodoId = m.PeriodoId }
            }).ToList());
        _mockMovRepo.Setup(repo => repo.ListarAsync(instituicaoId, periodoId)).ReturnsAsync(expected);
        

        // Act
        var result = await _controller.ListarAsync(instituicaoId, periodoId);

        // Assert
        var ok = Assert.IsType<OkObjectResult>(result);
        var returnedList = Assert.IsType<List<MovimentacaoResponse>>(ok.Value);
        Assert.Equal(expected.Count, returnedList.Count);
        
        for (int i = 0; i < expected.Count; i++)
        {
            Assert.Equal(expected[i].Id, returnedList[i].MovimentacaoId);
            Assert.Equal(expected[i].Data, returnedList[i].Data);
            Assert.Equal(expected[i].Descricao, returnedList[i].Descricao);
            Assert.Equal(expected[i].Valor, returnedList[i].Valor);
            Assert.Equal(expected[i].InstituicaoId, returnedList[i].Instituicao.InstituicaoId);
            Assert.Equal(expected[i].CategoriaId, returnedList[i].Categoria.CategoriaId);
            Assert.Equal(expected[i].PeriodoId, returnedList[i].Periodo.PeriodoId);
        }
    }

    [Fact]
    public async Task AdicionarAsync_ShouldReturnNotFound_WhenPeriodoNotFound()
    {
        // Arrange
        var request = CriarMovimentacaoRequest();
        _mockPeriodoRepo.Setup(r => r.ObterAsync(request.PeriodoId)).ReturnsAsync((Periodo?)null);

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
        _mockPeriodoRepo.Setup(r => r.ObterAsync(request.PeriodoId)).ReturnsAsync(new Periodo());
        _mockInstRepo.Setup(r => r.ObterAsync(request.InstituicaoId)).ReturnsAsync((Instituicao?)null);

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
        _mockPeriodoRepo.Setup(r => r.ObterAsync(request.PeriodoId)).ReturnsAsync(new Periodo());
        _mockInstRepo.Setup(r => r.ObterAsync(request.InstituicaoId)).ReturnsAsync(new Instituicao());
        _mockCatRepo.Setup(r => r.ObterAsync(request.CategoriaId)).ReturnsAsync((Categoria?)null);

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

        _mockConverterRequest.Setup(conv => conv.Convert(request))
            .Returns(new Movimentacao
            {
                Id = Guid.NewGuid(),
                Data = request.Data,
                Descricao = request.Descricao,
                Valor = request.Valor,
                InstituicaoId = request.InstituicaoId,
                CategoriaId = request.CategoriaId,
                PeriodoId = request.PeriodoId
            });

        _mockConverterResponse.Setup(conv => conv.Convert(It.IsAny<Movimentacao>()))
            .Returns((Movimentacao m) => new MovimentacaoResponse
            {
                MovimentacaoId = m.Id,
                Data = m.Data,
                Descricao = m.Descricao,
                Valor = m.Valor,
                Instituicao = new InstituicaoResponse { InstituicaoId = m.InstituicaoId },
                Categoria = new CategoriaResponse { CategoriaId = m.CategoriaId, Nome = "" },
                Periodo = new PeriodoResponse() { PeriodoId = m.PeriodoId }
            });

        _mockPeriodoRepo.Setup(r => r.ObterAsync(request.PeriodoId)).ReturnsAsync(new Periodo());
        _mockInstRepo.Setup(r => r.ObterAsync(request.InstituicaoId)).ReturnsAsync(new Instituicao
        {
            Id = request.InstituicaoId,
            SaldoAtual = saldoInicial,
            InstituicaoCredito = isCredito
        });
        _mockCatRepo.Setup(r => r.ObterAsync(request.CategoriaId)).ReturnsAsync(CriarCategoriaResponse());

        // Act
        var result = await _controller.AdicionarAsync(request);

        // Assert
        Assert.IsType<CreatedResult>(result);
        _mockMovRepo.Verify(r => r.AdicionarAsync(It.Is<Movimentacao>(s => s.Descricao == request.Descricao)), Times.Once);
        _mockInstRepo.Verify(r => r.AtualizarSaldoAsync(request.InstituicaoId, saldoInicial + expectedDelta), Times.Once);
    }

    [Fact]
    public async Task AdicionarAsync_ShouldReturnCreated_WithNoBody()
    {
        // Arrange
        var request = CriarMovimentacaoRequest();
        var saldoInicial = 200m;
        _mockPeriodoRepo.Setup(r => r.ObterAsync(request.PeriodoId)).ReturnsAsync(new Periodo());
        _mockInstRepo.Setup(r => r.ObterAsync(request.InstituicaoId)).ReturnsAsync(new Instituicao
        {
            Id = request.InstituicaoId,
            SaldoAtual = saldoInicial,
            InstituicaoCredito = false
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
        _mockMovRepo.Setup(r => r.ObterAsync(id)).ReturnsAsync((Movimentacao?)null);

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
        var mov = new Movimentacao {
            Id = id,
            Valor = 100,
            Instituicao = new Instituicao { Id = instituicaoId },
            InstituicaoId = instituicaoId
        };
        var inst = new Instituicao {
            Id = instituicaoId,
            SaldoInicial = 500,
            SaldoAtual = 500,
            InstituicaoCredito = false
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
        var mov = new Movimentacao
        {
            Id = id,
            Valor = 100,
            Instituicao = new Instituicao { Id = Guid.NewGuid() }
        };
        _mockMovRepo.Setup(r => r.ObterAsync(id)).ReturnsAsync(mov);
        _mockInstRepo.Setup(r => r.ObterAsync(mov.Instituicao.Id)).ReturnsAsync((Instituicao?)null);

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
        var instId = Guid.NewGuid();
        var mov = new Movimentacao
        {
            Id = id,
            Valor = 100,
            Instituicao = new Instituicao { Id = instId },
            InstituicaoId = instId
        };
        var saldoInicial = 500m;
        var inst = new Instituicao
        {
            Id = mov.Instituicao.Id,
            SaldoInicial = saldoInicial,
            SaldoAtual = saldoInicial,
            InstituicaoCredito = isCredito
        };
        _mockMovRepo.Setup(r => r.ObterAsync(id)).ReturnsAsync(mov);
        _mockInstRepo.Setup(r => r.ObterAsync(instId)).ReturnsAsync(inst);

        // Act
        var result = await _controller.DeletarAsync(id);

        // Assert
        var ok = Assert.IsType<OkResult>(result);
        _mockMovRepo.Verify(r => r.DeletarAsync(id), Times.Once);
        _mockInstRepo.Verify(r => r.AtualizarSaldoAsync(instId, saldoInicial + expectedDelta), Times.Once);
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

    private Categoria CriarCategoriaResponse()
    {
        return new Categoria
        {
            Id = Guid.NewGuid(),
            Nome = "Alimentos",
            ParentId = null
        };
    }
}
