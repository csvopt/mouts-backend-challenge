using Ambev.DeveloperEvaluation.Domain.Common;

namespace Ambev.DeveloperEvaluation.Domain.Entities;

/// <summary>
/// Represents an item in a sale and calculates its quantity-based discount.
/// </summary>
public class SaleItem : BaseEntity
{
    public Guid SaleId { get; private set; }
    public Guid ProductId { get; private set; }
    public string ProductName { get; private set; } = string.Empty;
    public int Quantity { get; private set; }
    public decimal UnitPrice { get; private set; }
    public decimal DiscountPercentage { get; private set; }
    public decimal TotalAmount { get; private set; }
    public bool IsCancelled { get; private set; }
    public DateTime? CancelledAt { get; private set; }
    public Sale Sale { get; private set; } = null!;

    private SaleItem()
    {
    }

    internal static SaleItem Create(
        Guid saleId,
        Guid productId,
        string productName,
        int quantity,
        decimal unitPrice)
    {
        Validate(productId, productName, quantity, unitPrice);

        return new SaleItem
        {
            Id = Guid.NewGuid(),
            SaleId = saleId,
            ProductId = productId,
            ProductName = productName.Trim(),
            Quantity = quantity,
            UnitPrice = unitPrice,
            DiscountPercentage = CalculateDiscount(quantity),
            TotalAmount = CalculateTotal(quantity, unitPrice)
        };
    }

    internal void Update(string productName, int quantity, decimal unitPrice)
    {
        Validate(ProductId, productName, quantity, unitPrice);

        ProductName = productName.Trim();
        Quantity = quantity;
        UnitPrice = unitPrice;
        DiscountPercentage = CalculateDiscount(quantity);
        TotalAmount = CalculateTotal(quantity, unitPrice);
    }

    public void Cancel()
    {
        if (IsCancelled)
            throw new DomainException("The sale item is already cancelled.");

        IsCancelled = true;
        CancelledAt = DateTime.UtcNow;
    }

    private static decimal CalculateDiscount(int quantity)
    {
        return quantity switch
        {
            >= 10 => 0.20m,
            >= 4 => 0.10m,
            _ => 0m
        };
    }

    private static decimal CalculateTotal(int quantity, decimal unitPrice)
    {
        return decimal.Round(
            quantity * unitPrice * (1 - CalculateDiscount(quantity)),
            2,
            MidpointRounding.AwayFromZero);
    }

    private static void Validate(
        Guid productId,
        string productName,
        int quantity,
        decimal unitPrice)
    {
        if (productId == Guid.Empty)
            throw new DomainException("Product identifier is required.");

        if (string.IsNullOrWhiteSpace(productName))
            throw new DomainException("Product name is required.");

        if (quantity <= 0)
            throw new DomainException("Product quantity must be greater than zero.");

        if (quantity > 20)
            throw new DomainException("It is not possible to sell more than 20 identical items.");

        if (unitPrice <= 0)
            throw new DomainException("Unit price must be greater than zero.");
    }
}
