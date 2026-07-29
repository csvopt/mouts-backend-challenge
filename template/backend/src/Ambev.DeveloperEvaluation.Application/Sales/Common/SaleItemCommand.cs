using FluentValidation;

namespace Ambev.DeveloperEvaluation.Application.Sales.Common;

public sealed class SaleItemCommand
{
    public Guid ProductId { get; set; }
    public string ProductName { get; set; } = string.Empty;
    public int Quantity { get; set; }
    public decimal UnitPrice { get; set; }
}

public sealed class SaleItemCommandValidator : AbstractValidator<SaleItemCommand>
{
    public SaleItemCommandValidator()
    {
        RuleFor(item => item.ProductId).NotEmpty();
        RuleFor(item => item.ProductName).NotEmpty().MaximumLength(150);
        RuleFor(item => item.Quantity).InclusiveBetween(1, 20);
        RuleFor(item => item.UnitPrice).GreaterThan(0);
    }
}
