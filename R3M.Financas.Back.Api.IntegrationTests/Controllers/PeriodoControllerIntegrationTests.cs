using R3M.Financas.Back.Api.IntegrationTests.Fixtures;
using R3M.Financas.Back.Domain.Dtos;
using System.Net;
using System.Net.Http.Json;

namespace R3M.Financas.Back.Api.IntegrationTests.Controllers;

[Collection("IntegrationTest collection")]
public class PeriodoControllerIntegrationTests : IntegrationTestsBase
{
    public const string ROTA_PERIODOS = "/api/periodo";

    public PeriodoControllerIntegrationTests(IntegrationTestFixture fixture)
    : base(fixture.Factory) { }

    [Fact]
    public async Task ListAsync_DeveRetornar200()
    {
        var rota = $"{ROTA_PERIODOS}/2025";
        var response = await _httpClient.GetAsync(rota);

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        
        var body = await response.Content.ReadFromJsonAsync<IEnumerable<PeriodoResponse>>();
        Assert.NotNull(body);
        Assert.Equal(12, body.Count());
    }

    [Fact]
    public async Task ListAsync_DeveRetornar200_QuandoRegistroNaoExistir()
    {
        var rota = $"{ROTA_PERIODOS}/2020";
        var response = await _httpClient.GetAsync(rota);

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var body = await response.Content.ReadFromJsonAsync<IEnumerable<PeriodoResponse>>();
        Assert.NotNull(body);
        Assert.Empty(body);
    }

    [Fact]
    public async Task ObterAsync_DeveRetornar200_QuandoRegistroExistir()
    {
        const string rota = $"{ROTA_PERIODOS}/ec69fc60-9f88-4f8c-9e16-aa8fb567f6ad";
        var response = await _httpClient.GetAsync(rota);

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var body = await response.Content.ReadFromJsonAsync<PeriodoResponse>();
        Assert.NotNull(body);
        Assert.Equal(Guid.Parse("ec69fc60-9f88-4f8c-9e16-aa8fb567f6ad"), body.PeriodoId);
        Assert.Equal("202502", body.Nome);
    }

    [Fact]
    public async Task ObterAsync_DeveRetornar400_QuandoRegistroNaoExistir()
    {
        const string rota = $"{ROTA_PERIODOS}/ec69fc60-9f88-4f8c-9e16-aa8fb567f6ff";
        var response = await _httpClient.GetAsync(rota);

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }
}
