using R3M.Financas.Back.Api.IntegrationTests.Fixtures;
using R3M.Financas.Back.Domain.Dtos;
using System.Net.Http.Json;

namespace R3M.Financas.Back.Api.IntegrationTests.Controllers;

[Collection("IntegrationTest collection")]
public class TipoCategoriaControllerIntegrationTests : IntegrationTestsBase
{
    public const string ROTA_TIPO_CATEGORIAS = "/api/tipocategoria";

    public TipoCategoriaControllerIntegrationTests(IntegrationTestFixture fixture)
    : base(fixture.Factory) { }

    [Fact]
    public async Task ListAsync_DeveRetornar200()
    {
        var response = await _httpClient.GetAsync(ROTA_TIPO_CATEGORIAS);

        response.EnsureSuccessStatusCode();
        var body = await response.Content.ReadFromJsonAsync<IEnumerable<TipoCategoriaResponse>>();

        Assert.NotNull(body);
        Assert.Equal(2, body.Count());
    }

    [Fact]
    public async Task ObterAsync_DeveRetornar200()
    {
        var id = Guid.Parse("84b26471-ae23-48e8-81dc-0db75846c832");
        var response = await _httpClient.GetAsync($"{ROTA_TIPO_CATEGORIAS}/{id}");

        response.EnsureSuccessStatusCode();
        var body = await response.Content.ReadFromJsonAsync<TipoCategoriaResponse>();

        Assert.NotNull(body);
        Assert.Equal(id, body.TipoCategoriaId);
    }
}
