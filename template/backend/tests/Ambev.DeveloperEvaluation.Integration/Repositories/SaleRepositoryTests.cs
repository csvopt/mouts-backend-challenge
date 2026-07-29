using Ambev.DeveloperEvaluation.Domain.Entities;
using Ambev.DeveloperEvaluation.Domain.Repositories;
using Ambev.DeveloperEvaluation.ORM;
using Ambev.DeveloperEvaluation.ORM.Repositories;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace Ambev.DeveloperEvaluation.Integration.Repositories;

public sealed class SaleRepositoryTests
{
    [Fact(DisplayName = "Given a persisted sale When querying and deleting it Then repository keeps consistency")]
    public async Task SaleRepository_CreateSearchAndDelete_KeepsConsistency()
    {
        // Given
        var options = new DbContextOptionsBuilder<DefaultContext>()
            .UseInMemoryDatabase($"sale-repository-{Guid.NewGuid()}")
            .Options;
        await using var context = new DefaultContext(options);
        var repository = new SaleRepository(context);
        var sale = Sale.Create(
            "SALE-INTEGRATION-001",
            DateTime.UtcNow,
            Guid.NewGuid(),
            "Integration Customer",
            Guid.NewGuid(),
            "Integration Branch");
        sale.AddItem(Guid.NewGuid(), "Integration Product", 10, 100m);

        // When
        await repository.CreateAsync(sale);
        var persistedSale = await repository.GetByIdAsync(sale.Id);
        var searchResult = await repository.SearchAsync(new SaleSearchCriteria(
            Page: 1,
            Size: 10,
            CustomerName: "*Customer*"));
        var deleted = await repository.DeleteAsync(sale.Id);

        // Then
        Assert.NotNull(persistedSale);
        Assert.Equal(800m, persistedSale.TotalAmount);
        Assert.Single(persistedSale.Items);
        Assert.Equal(1, searchResult.TotalCount);
        Assert.True(deleted);
        Assert.Null(await repository.GetByIdAsync(sale.Id));
    }
}
