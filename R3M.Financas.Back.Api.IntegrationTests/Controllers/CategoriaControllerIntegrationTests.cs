using R3M.Financas.Back.Api.Dto;
using R3M.Financas.Back.Api.IntegrationTests.Fixtures;
using System.Net.Http.Json;

namespace R3M.Financas.Back.Api.IntegrationTests.Controllers;

[Collection("IntegrationTest collection")]
public class CategoriaControllerIntegrationTests : IntegrationTestsBase
{
    public const string ROTA_CATEGORIAS = "/api/categoria";

    public CategoriaControllerIntegrationTests(IntegrationTestFixture fixture)
    : base(fixture.Factory) { }

    [Fact]
    public async Task ListAsync_DeveRetornar200()
    {
        var response = await _httpClient.GetAsync(ROTA_CATEGORIAS);

        response.EnsureSuccessStatusCode();
        var body = await response.Content.ReadFromJsonAsync<IEnumerable<CategoriaResponse>>();

        Assert.NotNull(body);
        Assert.Equal(8, body.Count());
    }

    [Fact]
    public async Task ListAsync_DeveRetornarRegistros_QuandoFiltrado()
    {
        // Arrange
        var url = $"{ROTA_CATEGORIAS}?nome=aba";

        // Act
        var response = await _httpClient.GetAsync(url);

        // Assert
        response.EnsureSuccessStatusCode();
        var body = await response.Content.ReadFromJsonAsync<IEnumerable<CategoriaResponse>>();

        Assert.NotNull(body);
        Assert.Equal(2, body.Count());

        Assert.Contains(body, c => c.Nome == "Abacate");
        Assert.Contains(body, c => c.Nome == "Abacaxi");
    }

    [Fact]
    public async Task ListByParentAsync_DeveRetornarApenasRegistrosSemPai_QuandoPaiNaoForInformado()
    {
        // Arrange
        var url = $"{ROTA_CATEGORIAS}/pai";

        // Act
        var response = await _httpClient.GetAsync(url);

        // Assert
        response.EnsureSuccessStatusCode();
        var body = await response.Content.ReadFromJsonAsync<IEnumerable<CategoriaResponse>>();

        Assert.NotNull(body);
        Assert.Equal(3, body.Count());

        Assert.Contains(body, c => c.Nome == "Pai 1");
        Assert.Contains(body, c => c.Nome == "Pai 2");
        Assert.Contains(body, c => c.Nome == "Vegetal");
    }

    [Fact]
    public async Task ListByParentAsync_DeveRetornarArvore_QuandoPaiForInformado()
    {
        // Arrange
        const string idFruta = "5bc2402a-8025-4e24-b75c-43346c942cf8";
        var url = $"{ROTA_CATEGORIAS}/pai/{idFruta}";

        // Act
        var response = await _httpClient.GetAsync(url);

        // Assert
        response.EnsureSuccessStatusCode();
        var body = await response.Content.ReadFromJsonAsync<IEnumerable<CategoriaResponse>>();

        Assert.NotNull(body);
        Assert.Equal(2, body.Count());

        Assert.Contains(body, c => c.Nome == "Abacate");
        Assert.Contains(body, c => c.Nome == "Abacaxi");
    }
}
