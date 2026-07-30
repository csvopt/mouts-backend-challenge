using Ambev.DeveloperEvaluation.Application.Sales.Common;
using Ambev.DeveloperEvaluation.Application.Sales.CreateSale;
using Bogus;

namespace Ambev.DeveloperEvaluation.Unit.Application.TestData;

public static class SaleCommandTestData
{
    private static readonly Faker Faker = new();

    public static CreateSaleCommand CreateValidCommand(int quantity = 4)
    {
        return new CreateSaleCommand
        {
            SaleNumber = $"SALE-{Faker.Random.AlphaNumeric(8).ToUpperInvariant()}",
            Date = DateTime.UtcNow,
            CustomerId = Guid.NewGuid(),
            CustomerName = Faker.Company.CompanyName(),
            BranchId = Guid.NewGuid(),
            BranchName = Faker.Address.City(),
            Items =
            [
                new SaleItemCommand
                {
                    ProductId = Guid.NewGuid(),
                    ProductName = Faker.Commerce.ProductName(),
                    Quantity = quantity,
                    UnitPrice = 100m
                }
            ]
        };
    }
}
