using Ambev.DeveloperEvaluation.Domain.Common;

namespace Ambev.DeveloperEvaluation.Domain.Entities;

/// <summary>
/// Represents a sale and protects the consistency of its items and totals.
/// </summary>
public class Sale : BaseEntity
{
    private readonly List<SaleItem> _items = [];

    public string SaleNumber { get; private set; } = string.Empty;
    public DateTime Date { get; private set; }
    public Guid CustomerId { get; private set; }
    public string CustomerName { get; private set; } = string.Empty;
    public Guid BranchId { get; private set; }
    public string BranchName { get; private set; } = string.Empty;
    public decimal TotalAmount { get; private set; }
    public bool IsCancelled { get; private set; }
    public DateTime? CancelledAt { get; private set; }
    public IReadOnlyCollection<SaleItem> Items => _items.AsReadOnly();

    private Sale()
    {
    }

    public static Sale Create(
        string saleNumber,
        DateTime date,
        Guid customerId,
        string customerName,
        Guid branchId,
        string branchName)
    {
        ValidateDetails(saleNumber, date, customerId, customerName, branchId, branchName);

        return new Sale
        {
            Id = Guid.NewGuid(),
            SaleNumber = saleNumber.Trim(),
            Date = date,
            CustomerId = customerId,
            CustomerName = customerName.Trim(),
            BranchId = branchId,
            BranchName = branchName.Trim()
        };
    }

    public SaleItem AddItem(Guid productId, string productName, int quantity, decimal unitPrice)
    {
        EnsureCanBeModified();

        if (_items.Any(item => item.ProductId == productId))
            throw new DomainException($"Product {productId} is already part of this sale.");

        var item = SaleItem.Create(Id, productId, productName, quantity, unitPrice);
        _items.Add(item);
        RecalculateTotal();

        return item;
    }

    public void UpdateDetails(
        string saleNumber,
        DateTime date,
        Guid customerId,
        string customerName,
        Guid branchId,
        string branchName)
    {
        EnsureCanBeModified();
        ValidateDetails(saleNumber, date, customerId, customerName, branchId, branchName);

        SaleNumber = saleNumber.Trim();
        Date = date;
        CustomerId = customerId;
        CustomerName = customerName.Trim();
        BranchId = branchId;
        BranchName = branchName.Trim();
    }

    public void ReplaceItems(IEnumerable<SaleItemData> items)
    {
        EnsureCanBeModified();

        var replacementItems = items.ToList();
        if (replacementItems.Count == 0)
            throw new DomainException("A sale must contain at least one item.");

        var duplicatedProduct = replacementItems
            .GroupBy(item => item.ProductId)
            .FirstOrDefault(group => group.Count() > 1);

        if (duplicatedProduct is not null)
            throw new DomainException($"Product {duplicatedProduct.Key} cannot be repeated in the same sale.");

        _items.RemoveAll(currentItem =>
            replacementItems.All(item => item.ProductId != currentItem.ProductId));

        foreach (var item in replacementItems)
        {
            var currentItem = _items.FirstOrDefault(existingItem =>
                existingItem.ProductId == item.ProductId);

            if (currentItem is null)
                _items.Add(SaleItem.Create(Id, item.ProductId, item.ProductName, item.Quantity, item.UnitPrice));
            else
                currentItem.Update(item.ProductName, item.Quantity, item.UnitPrice);
        }

        RecalculateTotal();
    }

    public void Cancel()
    {
        if (IsCancelled)
            throw new DomainException("The sale is already cancelled.");

        IsCancelled = true;
        CancelledAt = DateTime.UtcNow;
    }

    public void CancelItem(Guid itemId)
    {
        EnsureCanBeModified();

        var item = _items.FirstOrDefault(currentItem => currentItem.Id == itemId)
            ?? throw new DomainException($"Item {itemId} was not found in this sale.");

        item.Cancel();
        RecalculateTotal();
    }

    private static void ValidateDetails(
        string saleNumber,
        DateTime date,
        Guid customerId,
        string customerName,
        Guid branchId,
        string branchName)
    {
        if (string.IsNullOrWhiteSpace(saleNumber))
            throw new DomainException("Sale number is required.");

        if (date == default)
            throw new DomainException("Sale date is required.");

        if (customerId == Guid.Empty)
            throw new DomainException("Customer identifier is required.");

        if (string.IsNullOrWhiteSpace(customerName))
            throw new DomainException("Customer name is required.");

        if (branchId == Guid.Empty)
            throw new DomainException("Branch identifier is required.");

        if (string.IsNullOrWhiteSpace(branchName))
            throw new DomainException("Branch name is required.");
    }

    private void EnsureCanBeModified()
    {
        if (IsCancelled)
            throw new DomainException("A cancelled sale cannot be modified.");
    }

    private void RecalculateTotal()
    {
        TotalAmount = _items
            .Where(item => !item.IsCancelled)
            .Sum(item => item.TotalAmount);
    }
}

public sealed record SaleItemData(
    Guid ProductId,
    string ProductName,
    int Quantity,
    decimal UnitPrice);
