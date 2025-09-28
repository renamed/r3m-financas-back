using Microsoft.EntityFrameworkCore;
using R3M.Financas.Back.Api.IntegrationTests.Fixtures;
using R3M.Financas.Back.Domain.Dtos;
using System.Net;
using System.Net.Http.Json;
using System.Net.Mime;
using System.Text;
using System.Text.Json;

namespace R3M.Financas.Back.Api.IntegrationTests.Controllers;

[Collection("IntegrationTest collection")]
public class MovimentacaoControllerIntegrationTests : IntegrationTestsBase
{
    public const string ROTA_MOVIMENTACOES = "/api/movimentacao";

    public MovimentacaoControllerIntegrationTests(IntegrationTestFixture fixture)
    : base(fixture.Factory) { }

    [Fact]
    public async Task ListarAsync_DeveRetornar200_QuandoExistiremMovimentacoes()
    {
        // Arrage
        var rota = $"{ROTA_MOVIMENTACOES}/d2e81048-f5ec-49af-b053-30e61a4bafcd/7ffe9be3-0157-47e7-8f32-72f39ffed1ff";

        // Act
        var response = await _httpClient.GetAsync(rota);

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var body = await response.Content.ReadFromJsonAsync<IEnumerable<MovimentacaoResponse>>();
        Assert.Single(body);
    }

    [Fact]
    public async Task ListarAsync_DeveRetornar200_QuandoNaoExistiremMovimentacoes()
    {
        // Arrage
        var rota = $"{ROTA_MOVIMENTACOES}/d2e81048-f5ec-49af-b053-30e61a4bafcd/06f06331-139a-4b36-8fa6-cbf0448c09d7";

        // Act
        var response = await _httpClient.GetAsync(rota);

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var body = await response.Content.ReadFromJsonAsync<IEnumerable<MovimentacaoResponse>>();
        Assert.Empty(body);
    }

    [Fact]
    public async Task AdicionarAsync_DeveRetornar201_QuandoMovimentacaoForAdicionadaComSucesso()
    {
        // Arrange        
        using var scope = GetContext();
        var saldoInicial = (await
                scope
                .Instituicoes
                .AsNoTracking()
                .FirstAsync(f => 
                    f.Id == Guid.Parse("d2e81048-f5ec-49af-b053-30e61a4bafce"))).SaldoAtual;

        var movimentacaoRequest = new MovimentacaoRequest
        {
            InstituicaoId = Guid.Parse("d2e81048-f5ec-49af-b053-30e61a4bafce"),
            PeriodoId = Guid.Parse("7ffe9be3-0157-47e7-8f32-72f39ffed1ff"),
            CategoriaId = Guid.Parse("c91fc711-03a3-46da-9f40-8b745beecd90"),
            Data = new DateOnly(2025, 03, 16),
            Valor = -100.00m,
            Descricao = "Teste de movimentação"
        };
        
        var content = new StringContent(JsonSerializer.Serialize(movimentacaoRequest), Encoding.UTF8, MediaTypeNames.Application.Json);

        // Act
        var response = await _httpClient.PostAsync(ROTA_MOVIMENTACOES, content);

        // Assert
        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
        var saldoFinal = (await
                        scope
                        .Instituicoes
                        .AsNoTracking()
                        .FirstAsync(f =>
                            f.Id == Guid.Parse("d2e81048-f5ec-49af-b053-30e61a4bafce"))).SaldoAtual;

        Assert.Equal(saldoInicial - Math.Abs(movimentacaoRequest.Valor), saldoFinal);
    }

    [Fact]
    public async Task AdicionarAsync_DeveAtualizarSaldo_QuandoMovimentacaoForAdicionadaComSucesso()
    {
        // Arrange
        using var scope = GetContext();
        var instituicaoInicio = await scope
                            .Instituicoes
                            .AsNoTracking()
                            .FirstAsync(f =>
                                f.Id == Guid.Parse("08e1442c-a537-4f2c-9aaa-e63345e053cc"));

        var saldoInicial = instituicaoInicio.SaldoAtual;
        var limiteCreditoInicial = instituicaoInicio.LimiteCredito.Value;

        var movimentacaoRequest = new MovimentacaoRequest
        {
            InstituicaoId = Guid.Parse("08e1442c-a537-4f2c-9aaa-e63345e053cc"),
            PeriodoId = Guid.Parse("7ffe9be3-0157-47e7-8f32-72f39ffed1ff"),
            CategoriaId = Guid.Parse("c91fc711-03a3-46da-9f40-8b745beecd90"),
            Data = new DateOnly(2025, 03, 16),
            Valor = -100.00m,
            Descricao = "Teste de movimentação"
        };
        var content = new StringContent(JsonSerializer.Serialize(movimentacaoRequest), Encoding.UTF8, MediaTypeNames.Application.Json);

        // Act
        var response = await _httpClient.PostAsync(ROTA_MOVIMENTACOES, content);

        // Assert
        Assert.Equal(HttpStatusCode.Created, response.StatusCode);

        var instituicaoFinal = await scope
                              .Instituicoes
                              .AsNoTracking()
                              .FirstAsync(f =>
                                  f.Id == Guid.Parse("08e1442c-a537-4f2c-9aaa-e63345e053cc"));

        var saldoFinal = instituicaoFinal.SaldoAtual;
        var limiteCreditoFinal = instituicaoFinal.LimiteCredito.Value;

        Assert.Equal(saldoInicial + Math.Abs(movimentacaoRequest.Valor), saldoFinal);        
    }

    [Fact]
    public async Task DeletarAsync_DeveRetornar200()
    {
        // Arrange
        using var scope = GetContext();
        var movimentacao = await  GetContext()
            .Movimentacoes
            .AsNoTracking()
            .FirstOrDefaultAsync(f => f.Id != Guid.Parse("5190ec67-9b21-4a0e-a0a5-1dae5ce40e2b"));

        // Act
        var response = await _httpClient.DeleteAsync($"{ROTA_MOVIMENTACOES}/{movimentacao.Id}");

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }
}
