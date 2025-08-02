namespace R3M.Financas.Back.Api.IntegrationTests.Fixtures;

public class IntegrationTestFixture : IAsyncLifetime
{
    public FinancasBancoDeDadosFixture Database { get; }
    public FinancasWebApiFixture Factory { get; }

    public IntegrationTestFixture()
    {
        Database = new ();
        Factory = new ();
    }

    public async Task InitializeAsync()
    {
        await Database.InitializeAsync();
        Factory.ConnectionString = Database.ConnectionString;
    }

    public async Task DisposeAsync()
    {
        await Database.DisposeAsync();
        Factory.Dispose();
    }
}
