using Ambev.DeveloperEvaluation.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using System.Reflection;

namespace Ambev.DeveloperEvaluation.ORM;

public class DefaultContext : DbContext
{
    public DbSet<User> Users { get; set; }
    public DbSet<Sale> Sales { get; set; }
    public DbSet<SaleItem> SaleItems { get; set; }

    public DefaultContext(DbContextOptions<DefaultContext> options) : base(options)
    {
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfigurationsFromAssembly(Assembly.GetExecutingAssembly());
        base.OnModelCreating(modelBuilder);
    }
}
public class DefaultContextFactory : IDesignTimeDbContextFactory<DefaultContext>
{
    public DefaultContext CreateDbContext(string[] args)
    {
        var builder = new DbContextOptionsBuilder<DefaultContext>();
        var connectionString = Environment.GetEnvironmentVariable(
            "ConnectionStrings__DefaultConnection")
            ?? "Host=localhost;Port=5432;Database=developer_evaluation;Username=developer;Password=ev@luAt10n";

        builder.UseNpgsql(
            connectionString,
            options => options.MigrationsAssembly(typeof(DefaultContext).Assembly.FullName)
        );

        return new DefaultContext(builder.Options);
    }
}
