using R3M.Financas.Back.Api.Dto;
using R3M.Financas.Back.Api.IntegrationTests.Fixtures;
using System.Net;
using System.Net.Http.Json;

namespace R3M.Financas.Back.Api.IntegrationTests.Controllers;

[Collection("IntegrationTest collection")]
public class InstituicaoControllerIntegrationTests : IntegrationTestsBase
{
    public const string ROTA_INSTITUICOES = "/api/instituicao";

    public InstituicaoControllerIntegrationTests(IntegrationTestFixture fixture)
    : base(fixture.Factory) { }

    [Fact]
    public async Task ListAsync_DeveRetornar200()
    {
        var response = await _httpClient.GetAsync(ROTA_INSTITUICOES);

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        
        var body = await response.Content.ReadFromJsonAsync<IEnumerable<InstituicaoResponse>>();
        Assert.NotNull(body);
        Assert.Equal(3, body.Count());
    }

    [Fact]
    public async Task ObterAsync_DeveRetornar200_QuandoRegistroExistir()
    {
        const string rota = $"{ROTA_INSTITUICOES}/08e1442c-a537-4f2c-9aaa-e63345e053cc";
        var response = await _httpClient.GetAsync(rota);

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var body = await response.Content.ReadFromJsonAsync<InstituicaoResponse>();
        Assert.NotNull(body);
        Assert.Equal(Guid.Parse("08e1442c-a537-4f2c-9aaa-e63345e053cc"), body.InstituicaoId);
    }

    [Fact]
    public async Task ObterAsync_DeveRetornar400_QuandoRegistroNaoExistir()
    {
        const string rota = $"{ROTA_INSTITUICOES}/08e1442c-a537-4f2c-9aaa-e63345e053dd";
        var response = await _httpClient.GetAsync(rota);

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }
}
