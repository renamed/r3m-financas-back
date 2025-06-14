using Microsoft.AspNetCore.Mvc;
using Moq;
using R3M.Financas.Back.Api.Controllers;
using R3M.Financas.Back.Api.Dto;
using R3M.Financas.Back.Api.Interfaces;

namespace R3M.Financas.Back.Api.UnitTests.Controllers;

public class PeriodoControllerUnitTests
{
    private readonly Mock<IPeriodoRepository> _mockRepo;
    private readonly PeriodoController _controller;

    public PeriodoControllerUnitTests()
    {
        _mockRepo = new Mock<IPeriodoRepository>();
        _controller = new PeriodoController(_mockRepo.Object);
    }

    [Fact]
    public async Task ListarAsync_ShouldReturnPeriodos_ForGivenAnoBase()
    {
        // Arrange
        int anoBase = 2025;
        var expected = new List<PeriodoResponse>
            {
                new PeriodoResponse
                {
                    PeriodoId = Guid.NewGuid(),
                    Nome = "Janeiro",
                    Inicio = new DateOnly(2025, 1, 1),
                    Fim = new DateOnly(2025, 1, 31)
                },
                new PeriodoResponse
                {
                    PeriodoId = Guid.NewGuid(),
                    Nome = "Fevereiro",
                    Inicio = new DateOnly(2025, 2, 1),
                    Fim = new DateOnly(2025, 2, 28)
                }
            };

        _mockRepo.Setup(repo => repo.ListarAsync(anoBase))
                 .ReturnsAsync(expected);

        // Act
        var result = await _controller.ListarAsync(anoBase);

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result);
        Assert.Equal(expected, okResult.Value);
    }
}

