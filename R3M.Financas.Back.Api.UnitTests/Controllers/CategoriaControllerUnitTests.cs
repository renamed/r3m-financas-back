using Microsoft.AspNetCore.Mvc;
using Moq;
using R3M.Financas.Back.Api.Controllers;
using R3M.Financas.Back.Application.Interfaces;
using R3M.Financas.Back.Domain.Dtos;
using R3M.Financas.Back.Domain.Models;
using R3M.Financas.Back.Repository.Interfaces;

namespace R3M.Financas.Back.Api.UnitTests.Controllers;

public class CategoriaControllerUnitTests
{

    private readonly Mock<ICategoriaRepository> _mockCategoriaRepo;
    private readonly Mock<IMovimentacaoRepository> _mockMovimentacaoRepo;
    private readonly Mock<IConverter<CategoriaResponse, Categoria>> _mockResponseConverter;
    private readonly Mock<IConverter<CategoriaRequest, Categoria>> _mockRequestConverter;
    private readonly CategoriaController _controller;

    public CategoriaControllerUnitTests()
    {
        _mockCategoriaRepo = new Mock<ICategoriaRepository>();
        _mockMovimentacaoRepo = new Mock<IMovimentacaoRepository>();
        _mockResponseConverter = new Mock<IConverter<CategoriaResponse, Categoria>>();
        _mockRequestConverter = new Mock<IConverter<CategoriaRequest, Categoria>>();
        _controller = new CategoriaController(_mockCategoriaRepo.Object, _mockMovimentacaoRepo.Object, _mockResponseConverter.Object, _mockRequestConverter.Object);
    }

    [Fact]
    public async Task ListAsync_ShouldReturnAll_WhenNomeIsNull()
    {
        // Arrange
        var categorias = new List<Categoria>
        {
            new() { Id = Guid.NewGuid(), Nome = "Alimentos" },
            new() { Id = Guid.NewGuid(), Nome = "Transporte" }
        };

        var expected = categorias.Select(c => new CategoriaResponse { CategoriaId = c.Id, Nome = c.Nome }).ToList();

        _mockCategoriaRepo.Setup(r => r.ListAsync()).ReturnsAsync(categorias);
        _mockResponseConverter.Setup(c => c.BulkConvert(categorias)).Returns(expected);

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
        var categorias = new List<Categoria>
        {
            new() { Id = Guid.NewGuid(), Nome = "Aluguel" }
        };

        var expected = categorias.Select(c => new CategoriaResponse { CategoriaId = c.Id, Nome = c.Nome }).ToList();

        _mockCategoriaRepo.Setup(r => r.SearchAsync(nome)).ReturnsAsync(categorias);
        _mockResponseConverter.Setup(c => c.BulkConvert(categorias)).Returns(expected);

        // Act
        var result = await _controller.ListAsync(nome);

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result);
        Assert.Equal(expected, okResult.Value);
    }

    [Fact]
    public async Task ListByParentAsync_ShouldReturnCategorias_WhenParentIdProvided()
    {
        // Arrange
        var parentId = Guid.NewGuid();
        var categorias = new List<Categoria>
        {
            new() { Id = Guid.NewGuid(), Nome = "Internet", ParentId = parentId }
        };

        var expected = categorias.Select(c => new CategoriaResponse { CategoriaId = c.Id, Nome = c.Nome }).ToList();

        _mockCategoriaRepo.Setup(r => r.ListDirectChildrenAsync(parentId)).ReturnsAsync(categorias);
        _mockResponseConverter.Setup(c => c.BulkConvert(categorias)).Returns(expected);

        // Act
        var result = await _controller.ListByParentAsync(parentId);

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
        var request = new CategoriaRequest { Nome = nome };

        // Act
        var result = await _controller.CreateAsync(request);

        // Assert
        var badRequest = Assert.IsType<BadRequestObjectResult>(result);
        Assert.Equal("Nome é obrigatório", badRequest.Value);
    }

    [Theory]
    [InlineData("a")]
    [InlineData("aa")]
    [InlineData("aaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaa")]
    public async Task CreateAsync_ShouldReturnBadRequest_WhenNomeHasInvalidLength(string nome)
    {
        // Arrange
        var request = new CategoriaRequest { Nome = nome };

        // Act
        var result = await _controller.CreateAsync(request);

        // Assert
        var badRequest = Assert.IsType<BadRequestObjectResult>(result);
        Assert.Equal("Nome deve ter entre 3 e 80 caracteres", badRequest.Value);
    }

    [Fact]
    public async Task CreateAsync_ShouldReturnNotFound_WhenParentNotExists()
    {
        // Arrange
        var parentId = Guid.NewGuid();
        var request = new CategoriaRequest { Nome = "Lazer", ParentId = parentId };
        _mockCategoriaRepo.Setup(r => r.ObterAsync(parentId)).ReturnsAsync((Categoria?)null);

        // Act
        var result = await _controller.CreateAsync(request);

        // Assert
        var notFound = Assert.IsType<NotFoundObjectResult>(result);
        Assert.Equal("Categoria pai não encontrada", notFound.Value);
    }

    [Fact]
    public async Task CreateAsync_ShouldReturnCreated_WhenRequestValid()
    {
        // Arrange
        var request = new CategoriaRequest { Nome = "Lazer" };

        _mockRequestConverter.Setup(c => c.Convert(request)).Returns(new Categoria { Nome = request.Nome });

        // Act
        var result = await _controller.CreateAsync(request);

        // Assert
        Assert.IsType<CreatedResult>(result);
        _mockCategoriaRepo.Verify(r => r.AddAsync(It.Is<Categoria>(c => c.Nome == request.Nome)), Times.Once);
    }

    [Fact]
    public async Task DeleteAsync_ShouldReturnNotFound_WhenCategoriaDoesNotExist()
    {
        // Arrange
        var id = Guid.NewGuid();
        _mockCategoriaRepo.Setup(r => r.ObterAsync(id)).ReturnsAsync((Categoria?)null);

        // Act
        var result = await _controller.DeleteAsync(id);

        // Assert
        var notFound = Assert.IsType<NotFoundObjectResult>(result);
        Assert.Equal("Categoria não encontrada", notFound.Value);
    }

    [Fact]
    public async Task DeleteAsync_ShouldReturnBadRequest_WhenCategoriaHasChildren()
    {
        // Arrange
        var id = Guid.NewGuid();
        _mockCategoriaRepo.Setup(r => r.ObterAsync(id)).ReturnsAsync(new Categoria { Id = id, Nome = "Alimentos" });
        _mockCategoriaRepo.Setup(r => r.ListAllChildrenAsync(id))
            .ReturnsAsync([new() { Id = Guid.NewGuid(), Nome = "Frutas" }, new() { Id = Guid.NewGuid(), Nome = "Legumes" }]);

        // Act
        var result = await _controller.DeleteAsync(id);

        // Assert
        var badRequest = Assert.IsType<BadRequestObjectResult>(result);
        Assert.Equal("Não é possível excluir uma categoria que possui filhos.", badRequest.Value);
    }

    [Fact]
    public async Task DeleteAsync_ShouldReturnBadRequest_WhenCategoriaHasMovimentacoes()
    {
        // Arrange
        var id = Guid.NewGuid();
        _mockCategoriaRepo.Setup(r => r.ObterAsync(id)).ReturnsAsync(new Categoria { Id = id, Nome = "Alimentos" });
        _mockCategoriaRepo.Setup(r => r.ListAllChildrenAsync(id)).ReturnsAsync(new List<Categoria>());
        _mockMovimentacaoRepo.Setup(r => r.ContarPorCategoriaAsync(It.IsAny<IList<Guid>>())).ReturnsAsync(2);

        // Act
        var result = await _controller.DeleteAsync(id);

        // Assert
        var badRequest = Assert.IsType<BadRequestObjectResult>(result);
        Assert.Equal("Não é possível excluir uma categoria que possui movimentações associadas.", badRequest.Value);
    }

    [Fact]
    public async Task DeleteAsync_ShouldReturnNoContent_WhenDeletedSuccessfully()
    {
        // Arrange
        var id = Guid.NewGuid();
        _mockCategoriaRepo.Setup(r => r.ObterAsync(id)).ReturnsAsync(new Categoria { Id = id, Nome = "Alimentos" });
        _mockCategoriaRepo.Setup(r => r.ListAllChildrenAsync(id)).ReturnsAsync(new List<Categoria>());
        _mockMovimentacaoRepo.Setup(r => r.ContarPorCategoriaAsync(It.IsAny<IList<Guid>>())).ReturnsAsync(0);

        // Act
        var result = await _controller.DeleteAsync(id);

        // Assert
        Assert.IsType<NoContentResult>(result);
        _mockCategoriaRepo.Verify(r => r.DeleteAsync(id), Times.Once);
    }


    //private readonly Mock<ICategoriaRepository> _mockCategoriaRepo;
    //private readonly Mock<IMovimentacaoRepository> _mockMovimentacaoRepo;
    //private readonly CategoriaController _controller;

    //public CategoriaControllerUnitTests()
    //{
    //    _mockCategoriaRepo = new Mock<ICategoriaRepository>();
    //    _mockMovimentacaoRepo = new Mock<IMovimentacaoRepository>();
    //    _controller = new CategoriaController(_mockCategoriaRepo.Object, _mockMovimentacaoRepo.Object);
    //}

    //[Fact]
    //public async Task ListAsync_ShouldReturnAll_WhenNomeIsNull()
    //{
    //    // Arrange
    //    var expected = new List<Categoria>
    //        {
    //            new() { Id = Guid.NewGuid(), Nome = "Alimentos", ParentId = null },
    //            new() { Id = Guid.NewGuid(), Nome = "Transporte", ParentId = null }
    //        };
    //    _mockCategoriaRepo.Setup(repo => repo.ListAsync())
    //             .ReturnsAsync(expected);

    //    // Act
    //    var result = await _controller.ListAsync(null);

    //    // Assert
    //    var okResult = Assert.IsType<OkObjectResult>(result);
    //    Assert.Equal(expected, okResult.Value);
    //}

    //[Fact]
    //public async Task ListAsync_ShouldSearch_WhenNomeIsProvided()
    //{
    //    // Arrange
    //    var nome = "Moradia";
    //    var expected = new List<Categoria>
    //        {
    //            new() { Id = Guid.NewGuid(), Nome = "Aluguel", ParentId = null }
    //        };
    //    _mockCategoriaRepo.Setup(repo => repo.SearchAsync(nome))
    //             .ReturnsAsync(expected);

    //    // Act
    //    var result = await _controller.ListAsync(nome);

    //    // Assert
    //    var okResult = Assert.IsType<OkObjectResult>(result);
    //    Assert.Equal(expected, okResult.Value);
    //}

    //[Fact]
    //public async Task ListByParentAsync_ShouldReturnCategorias_WhenParentIdIsProvided()
    //{
    //    // Arrange
    //    var parentId = Guid.NewGuid();
    //    var expected = new List<Categoria>
    //        {
    //            new Categoria { Id = Guid.NewGuid(), Nome = "Internet", ParentId = parentId }
    //        };
    //    _mockCategoriaRepo.Setup(repo => repo.ListDirectChildrenAsync(parentId))
    //             .ReturnsAsync(expected);

    //    // Act
    //    var result = await _controller.ListByParentAsync(parentId);

    //    // Assert
    //    var okResult = Assert.IsType<OkObjectResult>(result);
    //    Assert.Equal(expected, okResult.Value);
    //}

    //[Fact]
    //public async Task ListByParentAsync_ShouldReturnCategorias_WhenParentIdIsNull()
    //{
    //    // Arrange
    //    var expected = new List<Categoria>
    //        {
    //            new Categoria { Id = Guid.NewGuid(), Nome = "Salário", ParentId = null }
    //        };
    //    _mockCategoriaRepo.Setup(repo => repo.ListDirectChildrenAsync(null))
    //             .ReturnsAsync(expected);

    //    // Act
    //    var result = await _controller.ListByParentAsync(null);

    //    // Assert
    //    var okResult = Assert.IsType<OkObjectResult>(result);
    //    Assert.Equal(expected, okResult.Value);
    //}

    //[Theory]
    //[InlineData(null)]
    //[InlineData("")]
    //public async Task CreateAsync_ShouldReturnBadRequest_WhenNomeIsMissing(string? nome)
    //{
    //    // Arrange
    //    var request = new CategoriaRequest { Nome = nome, ParentId = null };

    //    // Act
    //    var result = await _controller.CreateAsync(request);

    //    // Assert
    //    BadRequestObjectResult badRequest = Assert.IsType<BadRequestObjectResult>(result);
    //    badRequest.Value!.Equals("Nome é obrigatório");
    //}

    //[Theory]
    //[InlineData("a")]
    //[InlineData("aa")]
    //[InlineData("aaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaa")]
    //public async Task CreateAsync_ShouldReturnBadRequest_WhenNomeHasInvalidLength(string nome)
    //{
    //    // Arrange
    //    var request = new CategoriaRequest { Nome = nome, ParentId = null };

    //    // Act
    //    var result = await _controller.CreateAsync(request);

    //    // Assert
    //    BadRequestObjectResult badRequest = Assert.IsType<BadRequestObjectResult>(result);
    //    badRequest.Value!.Equals("Nome deve ter entre 3 e 80 caracteres");
    //}

    //[Fact]
    //public async Task CreateAsync_ShouldReturnNotFound_WhenParentCategoriaDoesNotExist()
    //{
    //    // Arrange
    //    var parentId = Guid.NewGuid();
    //    var request = new CategoriaRequest { Nome = "Lazer", ParentId = parentId };
    //    _mockCategoriaRepo.Setup(repo => repo.ObterAsync(parentId))
    //             .ReturnsAsync((Categoria?)null);

    //    // Act
    //    var result = await _controller.CreateAsync(request);

    //    // Assert
    //    NotFoundObjectResult notFound = Assert.IsType<NotFoundObjectResult>(result);
    //    notFound.Value!.Equals("Categoria pai não encontrada");
    //}

    //[Fact]
    //public async Task CreateAsync_ShouldReturnCreated_WhenRequestIsValid()
    //{
    //    // Arrange
    //    var request = new CategoriaRequest { Nome = "Lazer", ParentId = null };

    //    // Act
    //    var result = await _controller.CreateAsync(request);

    //    // Assert
    //    Assert.IsType<CreatedResult>(result);
    //    _mockCategoriaRepo.Verify(repo => repo.AddAsync(It.Is<Categoria>(s => s.Nome == request.Nome)), Times.Once);
    //}

    //[Fact]
    //public async Task DeleteAsync_ShouldReturnNotFound_WhenCategoriaDoesNotExist()
    //{
    //    // Arrange
    //    var Id = Guid.NewGuid();
    //    _mockCategoriaRepo.Setup(repo => repo.ObterAsync(Id))
    //             .ReturnsAsync((Categoria?)null);

    //    // Act
    //    var result = await _controller.DeleteAsync(Id);

    //    // Assert
    //    NotFoundObjectResult notFound = Assert.IsType<NotFoundObjectResult>(result);
    //    notFound.Value!.Equals("Categoria não encontrada");
    //}

    //[Fact]
    //public async Task DeleteAsync_ShouldReturnBadRequest_WhenCategoriaHasMovimentacoes()
    //{
    //    // Arrange
    //    var Id = Guid.NewGuid();
    //    _mockCategoriaRepo.Setup(repo => repo.ObterAsync(Id))
    //             .ReturnsAsync(new Categoria { Id = Id, Nome = "Alimentos" });

    //    _mockCategoriaRepo.Setup(repo => repo.ListAllChildrenAsync(Id))
    //             .ReturnsAsync([]);

    //    _mockMovimentacaoRepo.Setup(repo => repo.ContarPorCategoriaAsync(It.IsAny<IList<Guid>>()))
    //             .ReturnsAsync(3);

    //    // Act
    //    var result = await _controller.DeleteAsync(Id);

    //    // Assert
    //    BadRequestObjectResult badRequest = Assert.IsType<BadRequestObjectResult>(result);
    //    badRequest.Value!.Equals("Não é possível excluir uma categoria com movimentações associadas");
    //}

    //[Fact]
    //public async Task DeleteAsync_ShouldReurnBadRequest_WhenCategoriaHasChildren()
    //{
    //    // Arrange
    //    var Id = Guid.NewGuid();
    //    _mockCategoriaRepo.Setup(repo => repo.ObterAsync(Id))
    //             .ReturnsAsync(new Categoria { Id = Id, Nome = "Alimentos" });
    //    _mockCategoriaRepo.Setup(repo => repo.ListAllChildrenAsync(Id))
    //             .ReturnsAsync([ new (){ Id = Guid.NewGuid(), Nome = "Frutas" }, new() { Id = Guid.NewGuid(), Nome = "Legumes" }]);
    //    // Act
    //    var result = await _controller.DeleteAsync(Id);
    //    // Assert
    //    BadRequestObjectResult badRequest = Assert.IsType<BadRequestObjectResult>(result);
    //    badRequest.Value!.Equals("Não é possível excluir uma categoria que possui filhos.");
    //}

    //[Fact]
    //public async Task DeleteAsync_ShouldReturnNoContent_WhenCategoriaDeletedSuccessfully()
    //{
    //    // Arrange
    //    var Id = Guid.NewGuid();
    //    _mockCategoriaRepo.Setup(repo => repo.ObterAsync(Id))
    //             .ReturnsAsync(new Categoria { Id = Id, Nome = "Alimentos" });
    //    _mockCategoriaRepo.Setup(repo => repo.ListAllChildrenAsync(Id))
    //             .ReturnsAsync(new List<Categoria>());
    //    _mockMovimentacaoRepo.Setup(repo => repo.ContarPorCategoriaAsync(It.IsAny<IList<Guid>>()))
    //             .ReturnsAsync(0);
    //    // Act
    //    var result = await _controller.DeleteAsync(Id);

    //    // Assert
    //    Assert.IsType<NoContentResult>(result);
    //    _mockCategoriaRepo.Verify(repo => repo.DeleteAsync(Id), Times.Once);
    //}
}

