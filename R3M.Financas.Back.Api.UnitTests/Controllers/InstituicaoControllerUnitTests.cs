using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Moq;
using R3M.Financas.Back.Api.Controllers;
using R3M.Financas.Back.Application.Interfaces;
using R3M.Financas.Back.Domain.Dtos;
using R3M.Financas.Back.Domain.Models;
using R3M.Financas.Back.Repository.Interfaces;

namespace R3M.Financas.Back.Api.UnitTests.Controllers;

public class InstituicaoControllerUnitTests
{
    private readonly Mock<IInstituicaoRepository> _mockRepo;
    private readonly InstituicaoController _controller;

    private readonly Mock<IConverter<InstituicaoResponse, Instituicao>> _converterResponse;
    private readonly Mock<IConverter<InstituicaoRequest, Instituicao>> _converterRequest;

    public InstituicaoControllerUnitTests()
    {
        _mockRepo = new Mock<IInstituicaoRepository>();
        _converterResponse = new Mock<IConverter<InstituicaoResponse, Instituicao>>();
        _converterRequest = new Mock<IConverter<InstituicaoRequest, Instituicao>>();

        _controller = new InstituicaoController(_mockRepo.Object, _converterResponse.Object, _converterRequest.Object);
        _controller.ControllerContext = new ControllerContext { HttpContext = new DefaultHttpContext() };
    }

    [Fact]
    public async Task ListarAsync_ShouldReturnInstituicoes()
    {
        // Arrange
        var expected = new List<Instituicao>
            {
                new Instituicao
                {
                    Id = Guid.NewGuid(),
                    Nome = "Banco A",
                    SaldoInicial = 1000.50m,
                    InstituicaoCredito = true
                },
                new Instituicao
                {
                    Id = Guid.NewGuid(),
                    Nome = "Banco B",
                    SaldoInicial = 500.00m,
                    InstituicaoCredito = false
                }
            };

        _mockRepo.Setup(repo => repo.ListarAsync())
                 .ReturnsAsync(expected);

        _converterResponse.Setup(c => c.BulkConvert(expected)).Returns(new List<InstituicaoResponse>
            {
            new InstituicaoResponse
            {
                InstituicaoId = expected[0].Id,
                Nome = expected[0].Nome,
                Saldo = expected[0].SaldoInicial,
                Credito = expected[0].InstituicaoCredito,
                LimiteCredito = expected[0].LimiteCredito
            },
            new InstituicaoResponse
            {
                InstituicaoId = expected[1].Id,
                Nome = expected[1].Nome,
                Saldo = expected[1].SaldoInicial,
                Credito = expected[1].InstituicaoCredito,
                LimiteCredito = expected[1].LimiteCredito
            }
        });

        // Act
        var result = await _controller.ListarAsync();

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result);
        var response = Assert.IsType<List<InstituicaoResponse>>(okResult.Value);

        Assert.Equal(expected.Count, response.Count);
        for (int i = 0; i < expected.Count; i++)
        {
            Assert.Equal(expected[i].Id, response[i].InstituicaoId);
            Assert.Equal(expected[i].Nome, response[i].Nome);
            Assert.Equal(expected[i].SaldoInicial, response[i].Saldo);
            Assert.Equal(expected[i].InstituicaoCredito, response[i].Credito);
            Assert.Equal(expected[i].LimiteCredito, response[i].LimiteCredito);
        }
    }

    [Fact]
    public async Task ObterAsync_ShouldReturnInstituicao_WhenFound()
    {
        // Arrange
        var id = Guid.NewGuid();
        var expected = new Instituicao { Id = id, Nome = "Banco Teste", SaldoInicial = 100, InstituicaoCredito = true };
        _mockRepo.Setup(r => r.ObterAsync(id)).ReturnsAsync(expected);

       _converterResponse.Setup(c => c.Convert(expected)).Returns(new InstituicaoResponse
           {
            InstituicaoId = expected.Id,
            Nome = expected.Nome,
            Saldo = expected.SaldoInicial,
            Credito = expected.InstituicaoCredito,
            LimiteCredito = expected.LimiteCredito
        });

        // Act
        var result = await _controller.ObterAsync(id);

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result);
        var response = Assert.IsType<InstituicaoResponse>(okResult.Value);

        Assert.Equal(expected.Id, response.InstituicaoId);
        Assert.Equal(expected.Nome, response.Nome);
        Assert.Equal(expected.SaldoInicial, response.Saldo);
        Assert.Equal(expected.InstituicaoCredito, response.Credito);
        Assert.Equal(expected.LimiteCredito, response.LimiteCredito);
    }

    [Fact]
    public async Task ObterAsync_ShouldReturnNotFound_WhenNotFound()
    {
        // Arrange
        var id = Guid.NewGuid();
        _mockRepo.Setup(r => r.ObterAsync(id)).ReturnsAsync((Instituicao?)null);

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
    public async Task CriarAsync_ShouldReturnBadRequest_WhenLimiteCreditoZeroForInstituicaoCredito()
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
    public async Task CriarAsync_ShouldReturnBadRequest_WhenLimiteCreditoNegativeForInstituicaoCredito()
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
    public async Task CriarAsync_ShouldReturnBadRequest_WhenLimiteCreditoProvidedForNonInstituicaoCredito()
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
        var expected = new Instituicao {
            Id = Guid.NewGuid(),
            Nome = request.Nome,
            SaldoInicial = request.SaldoInicial,
            InstituicaoCredito = request.InstituicaoCredito,
            LimiteCredito = request.LimiteCredito
        };
        _mockRepo.Setup(r => r.ExistePorNomeAsync(request.Nome)).ReturnsAsync(false);
        _mockRepo.Setup(r => r.CriarAsync(It.Is<Instituicao>(i => i.Nome == request.Nome))).ReturnsAsync(expected);

        _converterRequest.Setup(c => c.Convert(request)).Returns(new Instituicao
        {
            Nome = request.Nome,
            SaldoInicial = request.SaldoInicial,
            InstituicaoCredito = request.InstituicaoCredito,
            LimiteCredito = request.LimiteCredito
        });

        _converterResponse.Setup(c => c.Convert(It.Is<Instituicao>(i => i.Nome == request.Nome))).Returns(new InstituicaoResponse
        {
            InstituicaoId = expected.Id,
            Nome = expected.Nome,
            Saldo = expected.SaldoInicial,
            Credito = expected.InstituicaoCredito,
            LimiteCredito = expected.LimiteCredito
        });

        // Act
        var result = await _controller.CriarAsync(request);
        // Assert
        var createdResult = Assert.IsType<CreatedResult>(result);
        var response = Assert.IsType<InstituicaoResponse>(createdResult.Value);

        Assert.Equal(expected.Id, response.InstituicaoId);
        Assert.Equal(expected.Nome, response.Nome);
        Assert.Equal(expected.SaldoInicial, response.Saldo);
        Assert.Equal(expected.InstituicaoCredito, response.Credito);
        Assert.Equal(expected.LimiteCredito, response.LimiteCredito);

    }

    [Fact]
    public async Task CriarAsync_ShouldReturnOk_WhenLimiteCreditoIsNullAndNotInstituicaoCredito()
    {
        // Arrange
        var request = new InstituicaoRequest {
            Nome = "Banco",
            DataSaldoInicial = DateOnly.FromDateTime(DateTime.Today),
            InstituicaoCredito = false,
            LimiteCredito = null,
            SaldoInicial = 100
        };
        var expected = new Instituicao {
            Id = Guid.NewGuid(),
            Nome = request.Nome,
            SaldoInicial = request.SaldoInicial,
            InstituicaoCredito = request.InstituicaoCredito,
            LimiteCredito = request.LimiteCredito
        };
        _mockRepo.Setup(r => r.ExistePorNomeAsync(request.Nome)).ReturnsAsync(false);
        _mockRepo.Setup(r => r.CriarAsync(It.Is<Instituicao>(s => s.Nome == request.Nome))).ReturnsAsync(expected);

        _converterRequest.Setup(c => c.Convert(request)).Returns(new Instituicao
        {
            Nome = request.Nome,
            SaldoInicial = request.SaldoInicial,
            InstituicaoCredito = request.InstituicaoCredito,
            LimiteCredito = request.LimiteCredito
        });

        _converterResponse.Setup(c => c.Convert(It.Is<Instituicao>(s => s.Nome == request.Nome))).Returns(new InstituicaoResponse
        {
            InstituicaoId = expected.Id,
            Nome = expected.Nome,
            Saldo = expected.SaldoInicial,
            Credito = expected.InstituicaoCredito,
            LimiteCredito = expected.LimiteCredito
        });

        // Act
        var result = await _controller.CriarAsync(request);
        // Assert
        var createdResult = Assert.IsType<CreatedResult>(result);
        var response = Assert.IsType<InstituicaoResponse>(createdResult.Value);

        Assert.Equal(expected.Id, response.InstituicaoId);
        Assert.Equal(expected.Nome, response.Nome);
        Assert.Equal(expected.SaldoInicial, response.Saldo);
        Assert.Equal(expected.InstituicaoCredito, response.Credito);
        Assert.Equal(expected.LimiteCredito, response.LimiteCredito);
    }
}

