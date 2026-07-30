using Ambev.DeveloperEvaluation.Application.Sales.Common;
using Ambev.DeveloperEvaluation.Application.Sales.CreateSale;
using Ambev.DeveloperEvaluation.Application.Sales.ListSales;
using Ambev.DeveloperEvaluation.Unit.Application.TestData;
using FluentValidation.TestHelper;
using Xunit;

namespace Ambev.DeveloperEvaluation.Unit.Application;

public sealed class SaleValidatorTests
{
    private readonly CreateSaleValidator _createValidator = new();
    private readonly ListSalesValidator _listValidator = new();

    [Fact(DisplayName = "Given repeated products When validating creation Then reports an error")]
    public void CreateSale_DuplicatedProducts_HasValidationError()
    {
        // Given
        var command = SaleCommandTestData.CreateValidCommand();
        var item = command.Items.Single();
        command.Items =
        [
            item,
            new SaleItemCommand
            {
                ProductId = item.ProductId,
                ProductName = item.ProductName,
                Quantity = 2,
                UnitPrice = item.UnitPrice
            }
        ];

        // When
        var result = _createValidator.TestValidate(command);

        // Then
        result.ShouldHaveValidationErrorFor(sale => sale.Items);
    }

    [Theory(DisplayName = "Given invalid quantity When validating creation Then reports an error")]
    [InlineData(0)]
    [InlineData(21)]
    public void CreateSale_InvalidQuantity_HasValidationError(int quantity)
    {
        // Given
        var command = SaleCommandTestData.CreateValidCommand(quantity);

        // When
        var result = _createValidator.TestValidate(command);

        // Then
        result.ShouldHaveValidationErrorFor("Items[0].Quantity");
    }

    [Theory(DisplayName = "Given supported ordering When validating listing Then succeeds")]
    [InlineData("date desc")]
    [InlineData("\"totalAmount desc, customerName asc\"")]
    [InlineData("saleNumber")]
    public void ListSales_SupportedOrdering_HasNoValidationError(string order)
    {
        // Given
        var query = new ListSalesQuery { Order = order };

        // When
        var result = _listValidator.TestValidate(query);

        // Then
        result.ShouldNotHaveValidationErrorFor(sale => sale.Order);
    }

    [Fact(DisplayName = "Given unsupported ordering When validating listing Then reports an error")]
    public void ListSales_UnsupportedOrdering_HasValidationError()
    {
        // Given
        var query = new ListSalesQuery { Order = "password desc" };

        // When
        var result = _listValidator.TestValidate(query);

        // Then
        result.ShouldHaveValidationErrorFor(sale => sale.Order);
    }
}
