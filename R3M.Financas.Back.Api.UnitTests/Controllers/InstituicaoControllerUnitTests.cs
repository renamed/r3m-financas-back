using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Moq;
using R3M.Financas.Back.Api.Controllers;
using R3M.Financas.Back.Api.Dto;
using R3M.Financas.Back.Api.Interfaces;

namespace R3M.Financas.Back.Api.UnitTests.Controllers;

public class InstituicaoControllerUnitTests
{
    private readonly Mock<IInstituicaoRepository> _mockRepo;
    private readonly InstituicaoController _controller;

    public InstituicaoControllerUnitTests()
    {
        _mockRepo = new Mock<IInstituicaoRepository>();
        _controller = new InstituicaoController(_mockRepo.Object);
        _controller.ControllerContext = new ControllerContext { HttpContext = new DefaultHttpContext() };
    }

    [Fact]
    public async Task ListarAsync_ShouldReturnInstituicoes()
    {
        // Arrange
        var expected = new List<InstituicaoResponse>
            {
                new InstituicaoResponse
                {
                    InstituicaoId = Guid.NewGuid(),
                    Nome = "Banco A",
                    Saldo = 1000.50m,
                    Credito = true
                },
                new InstituicaoResponse
                {
                    InstituicaoId = Guid.NewGuid(),
                    Nome = "Banco B",
                    Saldo = 500.00m,
                    Credito = false
                }
            };

        _mockRepo.Setup(repo => repo.ListarAsync())
                 .ReturnsAsync(expected);

        // Act
        var result = await _controller.ListarAsync();

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result);
        Assert.Equal(expected, okResult.Value);
    }

    [Fact]
    public async Task ObterAsync_ShouldReturnInstituicao_WhenFound()
    {
        // Arrange
        var id = Guid.NewGuid();
        var expected = new InstituicaoResponse { InstituicaoId = id, Nome = "Banco Teste", Saldo = 100, Credito = true };
        _mockRepo.Setup(r => r.ObterAsync(id)).ReturnsAsync(expected);

        // Act
        var result = await _controller.ObterAsync(id);

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result);
        Assert.Equal(expected, okResult.Value);
    }

    [Fact]
    public async Task ObterAsync_ShouldReturnNotFound_WhenNotFound()
    {
        // Arrange
        var id = Guid.NewGuid();
        _mockRepo.Setup(r => r.ObterAsync(id)).ReturnsAsync((InstituicaoResponse?)null);

        // Act
        var result = await _controller.ObterAsync(id);

        // Assert
        var notFoundResult = Assert.IsType<NotFoundObjectResult>(result);
        Assert.Contains($"{id}", notFoundResult.Value.ToString());
    }

    [Fact]
    public async Task CriarAsync_ShouldReturnBadRequest_WhenNomeIsNullOrWhiteSpace()
    {
        // Arrange
        var request = new InstituicaoRequest { Nome = " ", DataSaldoInicial = DateOnly.FromDateTime(DateTime.Today) };

        // Act
        var result = await _controller.CriarAsync(request);

        // Assert
        var response = Assert.IsType<BadRequestObjectResult>(result);
        Assert.Equal("O nome da instituição é obrigatório.", response.Value);
    }

    [Theory]
    [InlineData("AB")]
    [InlineData("A very long institution name that exceeds 25 chars")]
    public async Task CriarAsync_ShouldReturnBadRequest_WhenNomeLengthInvalid(string nome)
    {
        // Arrange
        var request = new InstituicaoRequest { Nome = nome, DataSaldoInicial = DateOnly.FromDateTime(DateTime.Today) };

        // Act
        var result = await _controller.CriarAsync(request);

        // Assert
        var response = Assert.IsType<BadRequestObjectResult>(result);
        Assert.Equal("O nome da instituição deve ter entre 3 e 25 caracteres.", response.Value);
    }

    [Fact]
    public async Task CriarAsync_ShouldReturnBadRequest_WhenDataSaldoInicialIsDefault()
    {
        // Arrange
        var request = new InstituicaoRequest { Nome = "Banco", DataSaldoInicial = default };

        // Act
        var result = await _controller.CriarAsync(request);

        // Assert
        var response = Assert.IsType<BadRequestObjectResult>(result);
        Assert.Equal("A data do saldo inicial é obrigatória.", response.Value);
    }

    [Fact]
    public async Task CriarAsync_ShouldReturnBadRequest_WhenLimiteCreditoZeroForCredito()
    {
        // Arrange
        var request = new InstituicaoRequest { Nome = "Banco", DataSaldoInicial = DateOnly.FromDateTime(DateTime.Today), InstituicaoCredito = true, LimiteCredito = 0 };

        // Act
        var result = await _controller.CriarAsync(request);

        // Assert
        var response = Assert.IsType<BadRequestObjectResult>(result);
        Assert.Equal("O limite de crédito deve ser informado se a instituição for de crédito.", response.Value);
    }

    [Fact]
    public async Task CriarAsync_ShouldReturnBadRequest_WhenLimiteCreditoNegativeForCredito()
    {
        // Arrange
        var request = new InstituicaoRequest { Nome = "Banco", DataSaldoInicial = DateOnly.FromDateTime(DateTime.Today), InstituicaoCredito = true, LimiteCredito = -10 };

        // Act
        var result = await _controller.CriarAsync(request);

        // Assert
        var response = Assert.IsType<BadRequestObjectResult>(result);
        Assert.Equal("O limite de crédito não pode ser negativo.", response.Value);
    }

    [Fact]
    public async Task CriarAsync_ShouldReturnBadRequest_WhenLimiteCreditoProvidedForNonCredito()
    {
        // Arrange
        var request = new InstituicaoRequest { Nome = "Banco", DataSaldoInicial = DateOnly.FromDateTime(DateTime.Today), InstituicaoCredito = false, LimiteCredito = 10 };

        // Act
        var result = await _controller.CriarAsync(request);

        // Assert
        var response = Assert.IsType<BadRequestObjectResult>(result);
        Assert.Equal("O limite de crédito não deve ser informado se a instituição não for de crédito.", response.Value);
    }

    [Fact]
    public async Task CriarAsync_ShouldReturnBadRequest_WhenNomeAlreadyExists()
    {
        // Arrange
        var request = new InstituicaoRequest 
        { 
            Nome = "Banco", 
            DataSaldoInicial = DateOnly.FromDateTime(DateTime.Today) 
        };
        _mockRepo.Setup(r => r.ExistePorNomeAsync(request.Nome)).ReturnsAsync(true);

        // Act
        var result = await _controller.CriarAsync(request);

        // Assert
        var response = Assert.IsType<BadRequestObjectResult>(result);
        Assert.Contains("Já existe uma instituição", response.Value.ToString());
    }

    [Fact]
    public async Task CriarAsync_ShouldReturnCreated_WhenValid()
    {
        // Arrange
        var request = new InstituicaoRequest {
            Nome = "Banco",
            DataSaldoInicial = DateOnly.FromDateTime(DateTime.Today),
            InstituicaoCredito = true,
            LimiteCredito = 100,
            SaldoInicial = 500
        };
        var expected = new InstituicaoResponse {
            InstituicaoId = Guid.NewGuid(),
            Nome = request.Nome,
            Saldo = request.SaldoInicial,
            Credito = request.InstituicaoCredito,
            LimiteCredito = request.LimiteCredito
        };
        _mockRepo.Setup(r => r.ExistePorNomeAsync(request.Nome)).ReturnsAsync(false);
        _mockRepo.Setup(r => r.CriarAsync(request)).ReturnsAsync(expected);

        // Act
        var result = await _controller.CriarAsync(request);
        // Assert
        var createdResult = Assert.IsType<CreatedResult>(result);
        Assert.Equal(expected, createdResult.Value);
    }

    [Fact]
    public async Task CriarAsync_ShouldReturnOk_WhenLimiteCreditoIsNullAndNotCredito()
    {
        // Arrange
        var request = new InstituicaoRequest {
            Nome = "Banco",
            DataSaldoInicial = DateOnly.FromDateTime(DateTime.Today),
            InstituicaoCredito = false,
            LimiteCredito = null,
            SaldoInicial = 100
        };
        var expected = new InstituicaoResponse {
            InstituicaoId = Guid.NewGuid(),
            Nome = request.Nome,
            Saldo = request.SaldoInicial,
            Credito = request.InstituicaoCredito,
            LimiteCredito = request.LimiteCredito
        };
        _mockRepo.Setup(r => r.ExistePorNomeAsync(request.Nome)).ReturnsAsync(false);
        _mockRepo.Setup(r => r.CriarAsync(request)).ReturnsAsync(expected);
        // Act
        var result = await _controller.CriarAsync(request);
        // Assert
        var createdResult = Assert.IsType<CreatedResult>(result);
        Assert.Equal(expected, createdResult.Value);
    }
}

