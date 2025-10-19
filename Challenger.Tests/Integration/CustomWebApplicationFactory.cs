using System;
using System.IO;
using System.Linq;
using Challenger.Infra.Data;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Challenger.App.Services;

namespace Challenger.Tests.Integration
{
    public class CustomWebApplicationFactory : WebApplicationFactory<Program>
    {
        protected override void ConfigureWebHost(IWebHostBuilder builder)
        {
            builder.UseEnvironment("Testing");

            builder.ConfigureServices(services =>
            {
                var descriptor = services.SingleOrDefault(d => d.ServiceType == typeof(DbContextOptions<SystemDbContext>));
                if (descriptor != null)
                {
                    services.Remove(descriptor);
                }

                services.AddDbContext<SystemDbContext>(options =>
                    options.UseInMemoryDatabase("Challenger_TestDb"));

                var evtDescriptor = services.SingleOrDefault(d => d.ServiceType == typeof(IEventProducer));
                if (evtDescriptor != null) services.Remove(evtDescriptor);
                services.AddSingleton<IEventProducer, NullEventProducer>();

                var storageDescriptor = services.SingleOrDefault(d => d.ServiceType == typeof(IStorageService));
                if (storageDescriptor != null) services.Remove(storageDescriptor);

                var tempBase = Path.Combine(Path.GetTempPath(), "challenger-tests-storage");
                Directory.CreateDirectory(tempBase);
                services.AddSingleton<IStorageService>(sp =>
                {
                    var configBuilder = new Microsoft.Extensions.Configuration.ConfigurationBuilder()
                        .AddInMemoryCollection(new[] {
                            new System.Collections.Generic.KeyValuePair<string,string>("Storage:Local:BasePath", tempBase)
                        });
                    var config = configBuilder.Build();
                    return new LocalStorageService(config);
                });

                var provider = services.BuildServiceProvider();
                using var scope = provider.CreateScope();
                var db = scope.ServiceProvider.GetRequiredService<SystemDbContext>();
                db.Database.EnsureDeleted();
                db.Database.EnsureCreated();
            });
        }
    }
}
