using Ambev.DeveloperEvaluation.Application.Sales.Common;
using MediatR;

namespace Ambev.DeveloperEvaluation.Application.Sales.CreateSale;

public sealed class CreateSaleCommand : IRequest<SaleResult>
{
    public string SaleNumber { get; set; } = string.Empty;
    public DateTime Date { get; set; }
    public Guid CustomerId { get; set; }
    public string CustomerName { get; set; } = string.Empty;
    public Guid BranchId { get; set; }
    public string BranchName { get; set; } = string.Empty;
    public IReadOnlyCollection<SaleItemCommand> Items { get; set; } = [];
}
