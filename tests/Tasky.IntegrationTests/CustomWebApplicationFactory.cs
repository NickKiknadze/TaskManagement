using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using Orleans;
using StackExchange.Redis;
using Tasky.Application.Interfaces;
using Tasky.Infrastructure.Persistence;

namespace Tasky.IntegrationTests;

public class CustomWebApplicationFactory : WebApplicationFactory<Program>
{
    private readonly string _dbName = Guid.NewGuid().ToString();

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.UseEnvironment("Testing");
        builder.ConfigureServices(services =>
        {
            var efServices = services.Where(d => 
                d.ServiceType == typeof(DbContextOptions<ApplicationDbContext>) ||
                d.ServiceType == typeof(DbContextOptions) ||
                d.ServiceType == typeof(IApplicationDbContext) ||
                d.ServiceType.Name.Contains("DbContext") ||
                d.ServiceType == typeof(IConnectionMultiplexer) ||
                d.ServiceType == typeof(IKafkaProducer) ||
                d.ServiceType == typeof(IClusterClient)).ToList();

            foreach (var descriptor in efServices)
            {
                services.Remove(descriptor);
            }

            services.AddDbContext<ApplicationDbContext>(options => 
            {
                options.UseInMemoryDatabase(_dbName);
                options.ConfigureWarnings(x => x.Ignore(Microsoft.EntityFrameworkCore.Diagnostics.InMemoryEventId.TransactionIgnoredWarning));
            });
            services.AddScoped<IApplicationDbContext>(sp => sp.GetRequiredService<ApplicationDbContext>());
            services.AddSingleton(new Mock<IConnectionMultiplexer>().Object);
            services.AddSingleton(new Mock<IKafkaProducer>().Object);
            services.AddSingleton(new Mock<IClusterClient>().Object);
        });
    }

    private void RemoveService(IServiceCollection services, Type serviceType)
    {
        var descriptors = services.Where(d => d.ServiceType == serviceType).ToList();
        foreach (var descriptor in descriptors)
        {
            services.Remove(descriptor);
        }
    }
}
