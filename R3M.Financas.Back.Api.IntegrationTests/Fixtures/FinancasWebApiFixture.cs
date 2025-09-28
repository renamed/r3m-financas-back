using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using R3M.Financas.Back.Repository.Contexts;

namespace R3M.Financas.Back.Api.IntegrationTests.Fixtures;

public class FinancasWebApiFixture 
    : WebApplicationFactory<Program>
{
    public string ConnectionString { get; set; }    

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {

        builder.ConfigureServices(services =>
        {
            // Remove the existing AuthDbContext registration
            var dbContextDescriptor = services.SingleOrDefault(
                d => d.ServiceType == typeof(DbContextOptions<FinancasContext>));
            if (dbContextDescriptor != null)
                services.Remove(dbContextDescriptor);

            // Register AuthDbContext with the test connection string
            services.AddDbContext<FinancasContext>(options =>
            {
                options.UseNpgsql(ConnectionString)
                       .UseSnakeCaseNamingConvention();
            });
        });

        builder.UseEnvironment("Development");
    }
}
