using Microsoft.AspNetCore.Mvc;
using Moq;
using R3M.Financas.Back.Api.Controllers;
using R3M.Financas.Back.Api.Dto;
using R3M.Financas.Back.Api.Interfaces;

namespace R3M.Financas.Back.Api.UnitTests.Controllers;

public class CategoriaControllerUnitTests
{
    private readonly Mock<ICategoriaRepository> _mockCategoriaRepo;
    private readonly Mock<IMovimentacaoRepository> _mockMovimentacaoRepo;
    private readonly CategoriaController _controller;

    public CategoriaControllerUnitTests()
    {
        _mockCategoriaRepo = new Mock<ICategoriaRepository>();
        _mockMovimentacaoRepo = new Mock<IMovimentacaoRepository>();
        _controller = new CategoriaController(_mockCategoriaRepo.Object, _mockMovimentacaoRepo.Object);
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
        _mockCategoriaRepo.Setup(repo => repo.ListAsync())
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
        _mockCategoriaRepo.Setup(repo => repo.SearchAsync(nome))
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
        _mockCategoriaRepo.Setup(repo => repo.ListDirectChildrenAsync(parentId))
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
        _mockCategoriaRepo.Setup(repo => repo.ListDirectChildrenAsync(null))
                 .ReturnsAsync(expected);

        // Act
        var result = await _controller.ListByParentAsync(null);

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result);
        Assert.Equal(expected, okResult.Value);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    public async Task CreateAsync_ShouldReturnBadRequest_WhenNomeIsMissing(string? nome)
    {
        // Arrange
        var request = new CategoriaRequest { Nome = nome, ParentId = null };

        // Act
        var result = await _controller.CreateAsync(request);

        // Assert
        BadRequestObjectResult badRequest = Assert.IsType<BadRequestObjectResult>(result);
        badRequest.Value!.Equals("Nome é obrigatório");
    }

    [Theory]
    [InlineData("a")]
    [InlineData("aa")]
    [InlineData("aaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaa")]
    public async Task CreateAsync_ShouldReturnBadRequest_WhenNomeHasInvalidLength(string nome)
    {
        // Arrange
        var request = new CategoriaRequest { Nome = nome, ParentId = null };

        // Act
        var result = await _controller.CreateAsync(request);

        // Assert
        BadRequestObjectResult badRequest = Assert.IsType<BadRequestObjectResult>(result);
        badRequest.Value!.Equals("Nome deve ter entre 3 e 80 caracteres");
    }

    [Fact]
    public async Task CreateAsync_ShouldReturnNotFound_WhenParentCategoriaDoesNotExist()
    {
        // Arrange
        var parentId = Guid.NewGuid();
        var request = new CategoriaRequest { Nome = "Lazer", ParentId = parentId };
        _mockCategoriaRepo.Setup(repo => repo.ObterAsync(parentId))
                 .ReturnsAsync((CategoriaResponse?)null);

        // Act
        var result = await _controller.CreateAsync(request);

        // Assert
        NotFoundObjectResult notFound = Assert.IsType<NotFoundObjectResult>(result);
        notFound.Value!.Equals("Categoria pai não encontrada");
    }

    [Fact]
    public async Task CreateAsync_ShouldReturnCreated_WhenRequestIsValid()
    {
        // Arrange
        var request = new CategoriaRequest { Nome = "Lazer", ParentId = null };

        // Act
        var result = await _controller.CreateAsync(request);

        // Assert
        Assert.IsType<CreatedResult>(result);
        _mockCategoriaRepo.Verify(repo => repo.AddAsync(request), Times.Once);
    }

    [Fact]
    public async Task DeleteAsync_ShouldReturnNotFound_WhenCategoriaDoesNotExist()
    {
        // Arrange
        var categoriaId = Guid.NewGuid();
        _mockCategoriaRepo.Setup(repo => repo.ObterAsync(categoriaId))
                 .ReturnsAsync((CategoriaResponse?)null);

        // Act
        var result = await _controller.DeleteAsync(categoriaId);

        // Assert
        NotFoundObjectResult notFound = Assert.IsType<NotFoundObjectResult>(result);
        notFound.Value!.Equals("Categoria não encontrada");
    }

    [Fact]
    public async Task DeleteAsync_ShouldReturnBadRequest_WhenCategoriaHasMovimentacoes()
    {
        // Arrange
        var categoriaId = Guid.NewGuid();
        _mockCategoriaRepo.Setup(repo => repo.ObterAsync(categoriaId))
                 .ReturnsAsync(new CategoriaResponse { CategoriaId = categoriaId, Nome = "Alimentos" });

        _mockCategoriaRepo.Setup(repo => repo.ListAllChildrenAsync(categoriaId))
                 .ReturnsAsync([]);

        _mockMovimentacaoRepo.Setup(repo => repo.ContarPorCategoriaAsync(It.IsAny<IList<Guid>>()))
                 .ReturnsAsync(3);

        // Act
        var result = await _controller.DeleteAsync(categoriaId);

        // Assert
        BadRequestObjectResult badRequest = Assert.IsType<BadRequestObjectResult>(result);
        badRequest.Value!.Equals("Não é possível excluir uma categoria com movimentações associadas");
    }

    [Fact]
    public async Task DeleteAsync_ShouldReurnBadRequest_WhenCategoriaHasChildren()
    {
        // Arrange
        var categoriaId = Guid.NewGuid();
        _mockCategoriaRepo.Setup(repo => repo.ObterAsync(categoriaId))
                 .ReturnsAsync(new CategoriaResponse { CategoriaId = categoriaId, Nome = "Alimentos" });
        _mockCategoriaRepo.Setup(repo => repo.ListAllChildrenAsync(categoriaId))
                 .ReturnsAsync([ new (){ CategoriaId = Guid.NewGuid(), Nome = "Frutas" }, new() { CategoriaId = Guid.NewGuid(), Nome = "Legumes" }]);
        // Act
        var result = await _controller.DeleteAsync(categoriaId);
        // Assert
        BadRequestObjectResult badRequest = Assert.IsType<BadRequestObjectResult>(result);
        badRequest.Value!.Equals("Não é possível excluir uma categoria que possui filhos.");
    }

    [Fact]
    public async Task DeleteAsync_ShouldReturnNoContent_WhenCategoriaDeletedSuccessfully()
    {
        // Arrange
        var categoriaId = Guid.NewGuid();
        _mockCategoriaRepo.Setup(repo => repo.ObterAsync(categoriaId))
                 .ReturnsAsync(new CategoriaResponse { CategoriaId = categoriaId, Nome = "Alimentos" });
        _mockCategoriaRepo.Setup(repo => repo.ListAllChildrenAsync(categoriaId))
                 .ReturnsAsync(new List<CategoriaResponse>());
        _mockMovimentacaoRepo.Setup(repo => repo.ContarPorCategoriaAsync(It.IsAny<IList<Guid>>()))
                 .ReturnsAsync(0);
        // Act
        var result = await _controller.DeleteAsync(categoriaId);

        // Assert
        Assert.IsType<NoContentResult>(result);
        _mockCategoriaRepo.Verify(repo => repo.DeleteAsync(categoriaId), Times.Once);
    }
}

