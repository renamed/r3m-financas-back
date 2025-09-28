using Microsoft.AspNetCore.Mvc;
using Moq;
using R3M.Financas.Back.Api.Controllers;
using R3M.Financas.Back.Application.Interfaces;
using R3M.Financas.Back.Domain.Dtos;
using R3M.Financas.Back.Domain.Models;
using R3M.Financas.Back.Repository.Interfaces;

namespace R3M.Financas.Back.Api.UnitTests.Controllers;

public class TipoCategoriaControllerUnitTests
{
    private readonly Mock<ITipoCategoriaRepository> _mockTipoCategoriaRepo;
    private readonly TipoCategoriaController _controller;
    private readonly Mock<IConverter<TipoCategoriaResponse, TipoCategoria>> _converter;

    public TipoCategoriaControllerUnitTests()
    {
        _mockTipoCategoriaRepo = new Mock<ITipoCategoriaRepository>();
        _converter = new Mock<IConverter<TipoCategoriaResponse, TipoCategoria>>();

        _controller = new TipoCategoriaController(_mockTipoCategoriaRepo.Object, _converter.Object);
    }

    [Fact]
    public async Task ListAsync_ShouldReturnAllTiposCategorias()
    {
        // Arrange
        var expected = new List<TipoCategoria>
        {
            new() { Id = Guid.NewGuid(), Nome = "Receita" },
            new() { Id = Guid.NewGuid(), Nome = "Despesa" }
        };
        _mockTipoCategoriaRepo.Setup(repo => repo.ListAsync())
            .ReturnsAsync(expected);
        _converter.Setup(conv => conv.BulkConvert(expected))
            .Returns(expected.Select(tc => new TipoCategoriaResponse { TipoCategoriaId = tc.Id, Nome = tc.Nome }).ToList());

        // Act
        var result = await _controller.ListAsync();
        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result);
        var returnedList = Assert.IsType<List<TipoCategoriaResponse>>(okResult.Value);

        Assert.Equal(expected.Count, returnedList.Count);
        for (int i = 0; i < expected.Count; i++)
        {
            Assert.Equal(expected[i].Id, returnedList[i].TipoCategoriaId);
            Assert.Equal(expected[i].Nome, returnedList[i].Nome);
        }
    }

    [Fact]
    public async Task ObterAsync_ShouldReturnTipoCategoria_WhenExists()
    {
        // Arrange
        var tipoCategoriaId = Guid.NewGuid();
        var expected = new TipoCategoria { Id = tipoCategoriaId, Nome = "Receita" };
        _mockTipoCategoriaRepo.Setup(repo => repo.ObterAsync(tipoCategoriaId))
            .ReturnsAsync(expected);
        _converter.Setup(conv => conv.Convert(expected))
            .Returns(new TipoCategoriaResponse { TipoCategoriaId = expected.Id, Nome = expected.Nome });

        // Act
        var result = await _controller.ObterAsync(tipoCategoriaId);

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result);

        var response = Assert.IsType<TipoCategoriaResponse>(okResult.Value);

        Assert.Equal(expected.Id, response.TipoCategoriaId);
        Assert.Equal(expected.Nome, response.Nome);
    }

    [Fact]
    public async Task ObterAsync_ShouldReturnNotFound_WhenTipoCategoriaDoesNotExist()
    {
        // Arrange
        var tipoCategoriaId = Guid.NewGuid();
        _mockTipoCategoriaRepo.Setup(repo => repo.ObterAsync(tipoCategoriaId))
            .ReturnsAsync((TipoCategoria?)null);

        // Act
        var result = await _controller.ObterAsync(tipoCategoriaId);

        // Assert
        var notFoundResult = Assert.IsType<NotFoundObjectResult>(result);
        Assert.Equal("Tipo de categoria não encontrado", notFoundResult.Value);
    }
}
