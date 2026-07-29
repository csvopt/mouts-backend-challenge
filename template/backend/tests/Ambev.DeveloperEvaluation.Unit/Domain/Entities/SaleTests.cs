using Ambev.DeveloperEvaluation.Domain;
using Ambev.DeveloperEvaluation.Domain.Entities;
using Ambev.DeveloperEvaluation.Unit.Domain.Entities.TestData;
using FluentAssertions;
using Xunit;

namespace Ambev.DeveloperEvaluation.Unit.Domain.Entities;

public class SaleTests
{
    [Theory(DisplayName = "Given an item quantity When adding it Then the correct discount is applied")]
    [InlineData(1, 0, 100)]
    [InlineData(3, 0, 300)]
    [InlineData(4, 10, 360)]
    [InlineData(9, 10, 810)]
    [InlineData(10, 20, 800)]
    [InlineData(20, 20, 1600)]
    public void AddItem_ValidQuantity_AppliesExpectedDiscount(
        int quantity,
        decimal expectedDiscountPercentage,
        decimal expectedTotal)
    {
        // Given
        var sale = SaleTestData.CreateSale();

        // When
        var item = SaleTestData.AddItem(sale, quantity);

        // Then
        item.DiscountPercentage.Should().Be(expectedDiscountPercentage / 100);
        item.TotalAmount.Should().Be(expectedTotal);
        sale.TotalAmount.Should().Be(expectedTotal);
    }

    [Theory(DisplayName = "Given an invalid item quantity When adding it Then a domain exception is thrown")]
    [InlineData(0)]
    [InlineData(-1)]
    [InlineData(21)]
    public void AddItem_InvalidQuantity_ThrowsDomainException(int quantity)
    {
        // Given
        var sale = SaleTestData.CreateSale();

        // When
        var act = () => SaleTestData.AddItem(sale, quantity);

        // Then
        act.Should().Throw<DomainException>();
    }

    [Fact(DisplayName = "Given multiple items When adding them Then the sale total is their sum")]
    public void AddItem_MultipleItems_CalculatesSaleTotal()
    {
        // Given
        var sale = SaleTestData.CreateSale();

        // When
        SaleTestData.AddItem(sale, quantity: 2, unitPrice: 25m);
        SaleTestData.AddItem(sale, quantity: 10, unitPrice: 10m);

        // Then
        sale.TotalAmount.Should().Be(130m);
    }

    [Fact(DisplayName = "Given the same product twice When adding it Then a domain exception is thrown")]
    public void AddItem_DuplicatedProduct_ThrowsDomainException()
    {
        // Given
        var sale = SaleTestData.CreateSale();
        var productId = Guid.NewGuid();
        sale.AddItem(productId, "Product", 1, 10m);

        // When
        var act = () => sale.AddItem(productId, "Product", 2, 10m);

        // Then
        act.Should().Throw<DomainException>()
            .WithMessage("*already part of this sale*");
    }

    [Fact(DisplayName = "Given an active item When cancelling it Then it is removed from the sale total")]
    public void CancelItem_ActiveItem_RecalculatesSaleTotal()
    {
        // Given
        var sale = SaleTestData.CreateSale();
        var cancelledItem = SaleTestData.AddItem(sale, quantity: 4, unitPrice: 10m);
        var activeItem = SaleTestData.AddItem(sale, quantity: 2, unitPrice: 15m);

        // When
        sale.CancelItem(cancelledItem.Id);

        // Then
        cancelledItem.IsCancelled.Should().BeTrue();
        cancelledItem.CancelledAt.Should().NotBeNull();
        sale.TotalAmount.Should().Be(activeItem.TotalAmount);
    }

    [Fact(DisplayName = "Given a cancelled sale When modifying it Then a domain exception is thrown")]
    public void AddItem_CancelledSale_ThrowsDomainException()
    {
        // Given
        var sale = SaleTestData.CreateSale();
        SaleTestData.AddItem(sale);
        sale.Cancel();

        // When
        var act = () => SaleTestData.AddItem(sale);

        // Then
        sale.IsCancelled.Should().BeTrue();
        sale.CancelledAt.Should().NotBeNull();
        act.Should().Throw<DomainException>()
            .WithMessage("A cancelled sale cannot be modified.");
    }

    [Fact(DisplayName = "Given duplicated replacement products When replacing items Then a domain exception is thrown")]
    public void ReplaceItems_DuplicatedProducts_ThrowsDomainException()
    {
        // Given
        var sale = SaleTestData.CreateSale();
        var productId = Guid.NewGuid();
        var items = new[]
        {
            new SaleItemData(productId, "Product", 1, 10m),
            new SaleItemData(productId, "Product", 2, 10m)
        };

        // When
        var act = () => sale.ReplaceItems(items);

        // Then
        act.Should().Throw<DomainException>()
            .WithMessage("*cannot be repeated*");
    }
}
