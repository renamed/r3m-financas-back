using Microsoft.AspNetCore.Mvc;
using Moq;
using R3M.Financas.Back.Api.Controllers;
using R3M.Financas.Back.Api.Dto;
using R3M.Financas.Back.Api.Interfaces;

namespace R3M.Financas.Back.Api.UnitTests.Controllers;

public class CategoriaControllerUnitTests
{
    private readonly Mock<ICategoriaRepository> _mockRepo;
    private readonly CategoriaController _controller;

    public CategoriaControllerUnitTests()
    {
        _mockRepo = new Mock<ICategoriaRepository>();
        _controller = new CategoriaController(_mockRepo.Object);
    }

    [Fact]
    public async Task ListAsync_ShouldReturnAll_WhenNomeIsNull()
    {
        // Arrange
        var expected = new List<CategoriaResponse>
            {
                new CategoriaResponse { CategoriaId = Guid.NewGuid(), Nome = "Alimentos", ParentId = null },
                new CategoriaResponse { CategoriaId = Guid.NewGuid(), Nome = "Transporte", ParentId = null }
            };
        _mockRepo.Setup(repo => repo.ListAsync())
                 .ReturnsAsync(expected);

        // Act
        var result = await _controller.ListAsync(null);

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result);
        Assert.Equal(expected, okResult.Value);
    }

    [Fact]
    public async Task ListAsync_ShouldSearch_WhenNomeIsProvided()
    {
        // Arrange
        var nome = "Moradia";
        var expected = new List<CategoriaResponse>
            {
                new CategoriaResponse { CategoriaId = Guid.NewGuid(), Nome = "Aluguel", ParentId = null }
            };
        _mockRepo.Setup(repo => repo.SearchAsync(nome))
                 .ReturnsAsync(expected);

        // Act
        var result = await _controller.ListAsync(nome);

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result);
        Assert.Equal(expected, okResult.Value);
    }

    [Fact]
    public async Task ListByParentAsync_ShouldReturnCategorias_WhenParentIdIsProvided()
    {
        // Arrange
        var parentId = Guid.NewGuid();
        var expected = new List<CategoriaResponse>
            {
                new CategoriaResponse { CategoriaId = Guid.NewGuid(), Nome = "Internet", ParentId = parentId }
            };
        _mockRepo.Setup(repo => repo.ListAsync(parentId))
                 .ReturnsAsync(expected);

        // Act
        var result = await _controller.ListByParentAsync(parentId);

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result);
        Assert.Equal(expected, okResult.Value);
    }

    [Fact]
    public async Task ListByParentAsync_ShouldReturnCategorias_WhenParentIdIsNull()
    {
        // Arrange
        var expected = new List<CategoriaResponse>
            {
                new CategoriaResponse { CategoriaId = Guid.NewGuid(), Nome = "Salário", ParentId = null }
            };
        _mockRepo.Setup(repo => repo.ListAsync(null))
                 .ReturnsAsync(expected);

        // Act
        var result = await _controller.ListByParentAsync(null);

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result);
        Assert.Equal(expected, okResult.Value);
    }
}

