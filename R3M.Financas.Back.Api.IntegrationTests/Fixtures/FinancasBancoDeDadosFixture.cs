using DotNet.Testcontainers.Builders;
using Testcontainers.PostgreSql;

namespace R3M.Financas.Back.Api.IntegrationTests.Fixtures;

public class FinancasBancoDeDadosFixture : IAsyncLifetime
{
    public PostgreSqlContainer PostgreSqlContainer { get; private set; }
    public string ConnectionString => PostgreSqlContainer.GetConnectionString();

    public async Task InitializeAsync()
    {
        PostgreSqlContainer = new PostgreSqlBuilder()
            .WithImage("postgres:17.5")
            .WithDatabase("oauth")
            .WithUsername("testuser")
            .WithPassword("testpassword")
            .WithPortBinding(5700, 5432)
            .WithWaitStrategy(Wait.ForUnixContainer().UntilMessageIsLogged("database system is ready to accept connections"))
            .Build();

        await PostgreSqlContainer.StartAsync();
    }

    public async Task DisposeAsync()
    {
        await PostgreSqlContainer.DisposeAsync();
    }
}
