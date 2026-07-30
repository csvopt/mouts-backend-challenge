using Ambev.DeveloperEvaluation.Domain.Entities;
using Bogus;

namespace Ambev.DeveloperEvaluation.Unit.Domain.Entities.TestData;

public static class SaleTestData
{
    private static readonly Faker Faker = new();

    public static Sale CreateSale()
    {
        return Sale.Create(
            $"SALE-{Faker.Random.AlphaNumeric(8).ToUpperInvariant()}",
            DateTime.UtcNow,
            Guid.NewGuid(),
            Faker.Company.CompanyName(),
            Guid.NewGuid(),
            Faker.Address.City());
    }

    public static SaleItem AddItem(Sale sale, int quantity = 1, decimal unitPrice = 100m)
    {
        return sale.AddItem(
            Guid.NewGuid(),
            Faker.Commerce.ProductName(),
            quantity,
            unitPrice);
    }
}
