using Ambev.DeveloperEvaluation.Application.Sales.Common;
using FluentValidation;

namespace Ambev.DeveloperEvaluation.Application.Sales.CreateSale;

public sealed class CreateSaleValidator : AbstractValidator<CreateSaleCommand>
{
    public CreateSaleValidator()
    {
        RuleFor(sale => sale.SaleNumber).NotEmpty().MaximumLength(50);
        RuleFor(sale => sale.Date).NotEmpty();
        RuleFor(sale => sale.CustomerId).NotEmpty();
        RuleFor(sale => sale.CustomerName).NotEmpty().MaximumLength(150);
        RuleFor(sale => sale.BranchId).NotEmpty();
        RuleFor(sale => sale.BranchName).NotEmpty().MaximumLength(150);
        RuleFor(sale => sale.Items)
            .NotEmpty()
            .Must(HaveUniqueProducts)
            .WithMessage("Products cannot be repeated in the same sale.");
        RuleForEach(sale => sale.Items).SetValidator(new SaleItemCommandValidator());
    }

    private static bool HaveUniqueProducts(IReadOnlyCollection<SaleItemCommand> items)
    {
        return items.Select(item => item.ProductId).Distinct().Count() == items.Count;
    }
}
