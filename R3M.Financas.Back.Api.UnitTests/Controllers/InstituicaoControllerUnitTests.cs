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
}

