using Microsoft.Extensions.DependencyInjection;
using R3M.Financas.Back.Api.Data;
using R3M.Financas.Back.Api.Modelos;
using System.Text.Json;

namespace R3M.Financas.Back.Api.IntegrationTests.Fixtures;

public class IntegrationTestsBase
{    
    private readonly FinancasWebApiFixture _factory;

    protected HttpClient _httpClient { get; private set; }

    private static bool populated = false;
    public IntegrationTestsBase(FinancasWebApiFixture factory)
    {
        _httpClient = factory.CreateClient();
        _factory = factory;
                
        var Context = GetContext();

        EnsureDatabaseCreated(Context);
        PopulateDb();
    }

    protected FinancasContext GetContext()
    {
        var scope = _factory.Services.CreateScope();
        return scope.ServiceProvider.GetRequiredService<FinancasContext>();
    }

    private static void EnsureDatabaseCreated(FinancasContext context)
    {
        context.Database.EnsureCreated();
    }

    private void PopulateDb()
    {
        if (populated) return;

        var context = GetContext();
        context.Categorias.AddRange(LerDados<Categoria>("categorias.json"));
        context.Instituicoes.AddRange(LerDados<Instituicao>("instituicoes.json"));
        context.Periodos.AddRange(LerDados<Periodo>("periodos.json"));
        context.Movimentacoes.AddRange(LerDados<Movimentacao>("movimentacoes.json"));

        context.SaveChanges();
        populated = true;
    }

    private static IEnumerable<T> LerDados<T>(string nomeArquivo)
    {
        var caminhoArquivo = Path.Combine("Dados", nomeArquivo);
        var conteudo = File.ReadAllText(caminhoArquivo);
        return JsonSerializer.Deserialize<IEnumerable<T>>(conteudo, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
    }
} 
