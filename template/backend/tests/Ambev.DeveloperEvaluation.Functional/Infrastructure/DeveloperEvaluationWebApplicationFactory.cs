using Ambev.DeveloperEvaluation.ORM;
using Ambev.DeveloperEvaluation.WebApi;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace Ambev.DeveloperEvaluation.Functional.Infrastructure;

public sealed class DeveloperEvaluationWebApplicationFactory
    : WebApplicationFactory<Program>
{
    private readonly string _databaseName = $"developer-evaluation-{Guid.NewGuid()}";

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.UseEnvironment("Testing");
        builder.ConfigureServices(services =>
        {
            services.RemoveAll<DbContextOptions<DefaultContext>>();
            services.RemoveAll<DefaultContext>();
            services.AddDbContext<DefaultContext>(options =>
                options.UseInMemoryDatabase(_databaseName));
        });
    }
}
