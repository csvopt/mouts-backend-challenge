using AutoMapper;
using Ambev.DeveloperEvaluation.Domain.Entities;

namespace Ambev.DeveloperEvaluation.Application.Sales.Common;

public sealed class SaleResult
{
    public Guid Id { get; set; }
    public string SaleNumber { get; set; } = string.Empty;
    public DateTime Date { get; set; }
    public Guid CustomerId { get; set; }
    public string CustomerName { get; set; } = string.Empty;
    public Guid BranchId { get; set; }
    public string BranchName { get; set; } = string.Empty;
    public decimal TotalAmount { get; set; }
    public bool IsCancelled { get; set; }
    public DateTime? CancelledAt { get; set; }
    public IReadOnlyCollection<SaleItemResult> Items { get; set; } = [];
}

public sealed class SaleItemResult
{
    public Guid Id { get; set; }
    public Guid ProductId { get; set; }
    public string ProductName { get; set; } = string.Empty;
    public int Quantity { get; set; }
    public decimal UnitPrice { get; set; }
    public decimal DiscountPercentage { get; set; }
    public decimal TotalAmount { get; set; }
    public bool IsCancelled { get; set; }
    public DateTime? CancelledAt { get; set; }
}

public sealed class SaleResultProfile : Profile
{
    public SaleResultProfile()
    {
        CreateMap<Sale, SaleResult>();
        CreateMap<SaleItem, SaleItemResult>();
    }
}
