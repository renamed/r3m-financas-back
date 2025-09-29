using Microsoft.AspNetCore.Mvc;
using Moq;
using R3M.Financas.Back.Api.Controllers;
using R3M.Financas.Back.Application.Interfaces;
using R3M.Financas.Back.Domain.Dtos;
using R3M.Financas.Back.Domain.Models;
using R3M.Financas.Back.Repository.Interfaces;

namespace R3M.Financas.Back.Api.UnitTests.Controllers;

public class PeriodoControllerUnitTests
{
    private readonly Mock<IPeriodoRepository> _mockRepo;
    private readonly Mock<IConverter<PeriodoResponse, Periodo>> _mockConverter;
    private readonly PeriodoController _controller;

    public PeriodoControllerUnitTests()
    {
        _mockRepo = new Mock<IPeriodoRepository>();
        _mockConverter = new Mock<IConverter<PeriodoResponse, Periodo>>();
        _controller = new PeriodoController(_mockRepo.Object, _mockConverter.Object);
    }

    [Fact]
    public async Task ListarAsync_ShouldReturnPeriodos_ForGivenAnoBase()
    {
        // Arrange
        int anoBase = 2025;
        var expected = new List<Periodo>
        {
            new Periodo
            {
                Id = Guid.NewGuid(),
                Nome = "Janeiro",
                Inicio = new DateOnly(2025, 1, 1),
                Fim = new DateOnly(2025, 1, 31)
            },
            new Periodo
            {
                Id = Guid.NewGuid(),
                Nome = "Fevereiro",
                Inicio = new DateOnly(2025, 2, 1),
                Fim = new DateOnly(2025, 2, 28)
            }
        };

        _mockRepo.Setup(repo => repo.ListarAsync(anoBase))
                 .ReturnsAsync(expected);

        _mockConverter.Setup(c => c.BulkConvert(expected)).Returns(expected.Select(p => new PeriodoResponse
        {
            Fim = p.Fim,
            Inicio = p.Inicio,
            Nome = p.Nome,
            PeriodoId = p.Id
        }).ToList());

        // Act
        var result = await _controller.ListarAsync(anoBase);

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result);
        var resultList = Assert.IsType<List<PeriodoResponse>>(okResult.Value);

        Assert.Equal(expected.Count, resultList.Count);
        for (int i = 0; i < expected.Count; i++)
        {
            Assert.Equal(expected[i].Id, resultList[i].PeriodoId);
            Assert.Equal(expected[i].Nome, resultList[i].Nome);
            Assert.Equal(expected[i].Inicio, resultList[i].Inicio);
            Assert.Equal(expected[i].Fim, resultList[i].Fim);
        }
    }

    [Fact]
    public async Task ObterAsync_ShouldReturnNotFound_WhenPeriodoDoesNotExist()
    {
        // Arrange
        var id = Guid.NewGuid();
        _mockRepo.Setup(r => r.ObterAsync(id)).ReturnsAsync((Periodo?)null);

        // Act
        var result = await _controller.ObterAsync(id);

        // Assert
        var notFound = Assert.IsType<NotFoundObjectResult>(result);
        Assert.Equal($"Período com ID {id} não encontrado.", notFound.Value);
    }

    [Fact]
    public async Task ObterAsync_ShouldReturnPeriodoResponse_WhenPeriodoExists()
    {
        // Arrange
        var id = Guid.NewGuid();
        var periodo = new Periodo
        {
            Id = id,
            Nome = "Março",
            Inicio = new DateOnly(2025, 3, 1),
            Fim = new DateOnly(2025, 3, 31)
        };

        var expectedResponse = new PeriodoResponse
        {
            PeriodoId = periodo.Id,
            Nome = periodo.Nome,
            Inicio = periodo.Inicio,
            Fim = periodo.Fim
        };

        _mockRepo.Setup(r => r.ObterAsync(id)).ReturnsAsync(periodo);
        _mockConverter.Setup(c => c.Convert(periodo)).Returns(expectedResponse);

        // Act
        var result = await _controller.ObterAsync(id);

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result);
        Assert.Equal(expectedResponse, okResult.Value);
    }
}