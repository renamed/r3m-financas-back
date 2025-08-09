using Microsoft.AspNetCore.Mvc;
using Moq;
using R3M.Financas.Back.Api.Controllers;
using R3M.Financas.Back.Api.Dto;
using R3M.Financas.Back.Api.Interfaces;

namespace R3M.Financas.Back.Api.UnitTests.Controllers;

public class TipoCategoriaControllerUnitTests
{
    private readonly Mock<ITipoCategoriaRepository> _mockTipoCategoriaRepo;
    private readonly TipoCategoriaController _controller;

    public TipoCategoriaControllerUnitTests()
    {
        _mockTipoCategoriaRepo = new Mock<ITipoCategoriaRepository>();
        _controller = new TipoCategoriaController(_mockTipoCategoriaRepo.Object);
    }

    [Fact]
    public async Task ListAsync_ShouldReturnAllTiposCategorias()
    {
        // Arrange
        var expected = new List<TipoCategoriaResponse>
        {
            new() { TipoCategoriaId = Guid.NewGuid(), Nome = "Receita" },
            new() { TipoCategoriaId = Guid.NewGuid(), Nome = "Despesa" }
        };
        _mockTipoCategoriaRepo.Setup(repo => repo.ListAsync())
            .ReturnsAsync(expected);

        // Act
        var result = await _controller.ListAsync();
        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result);
        Assert.Equal(expected, okResult.Value);
    }

    [Fact]
    public async Task ObterAsync_ShouldReturnTipoCategoria_WhenExists()
    {
        // Arrange
        var tipoCategoriaId = Guid.NewGuid();
        var expected = new TipoCategoriaResponse { TipoCategoriaId = tipoCategoriaId, Nome = "Receita" };
        _mockTipoCategoriaRepo.Setup(repo => repo.ObterAsync(tipoCategoriaId))
            .ReturnsAsync(expected);

        // Act
        var result = await _controller.ObterAsync(tipoCategoriaId);

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result);
        Assert.Equal(expected, okResult.Value);
    }

    [Fact]
    public async Task ObterAsync_ShouldReturnNotFound_WhenTipoCategoriaDoesNotExist()
    {
        // Arrange
        var tipoCategoriaId = Guid.NewGuid();
        _mockTipoCategoriaRepo.Setup(repo => repo.ObterAsync(tipoCategoriaId))
            .ReturnsAsync((TipoCategoriaResponse?)null);

        // Act
        var result = await _controller.ObterAsync(tipoCategoriaId);

        // Assert
        var notFoundResult = Assert.IsType<NotFoundObjectResult>(result);
        Assert.Equal("Tipo de categoria não encontrado", notFoundResult.Value);
    }
}
