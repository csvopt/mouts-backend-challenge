using Ambev.DeveloperEvaluation.Application.Sales.Common;
using FluentValidation;
using MediatR;

namespace Ambev.DeveloperEvaluation.Application.Sales.ListSales;

public sealed class ListSalesQuery : IRequest<PaginatedSaleResult>
{
    public int Page { get; set; } = 1;
    public int Size { get; set; } = 10;
    public string? Order { get; set; }
    public string? SaleNumber { get; set; }
    public string? CustomerName { get; set; }
    public string? BranchName { get; set; }
    public bool? IsCancelled { get; set; }
    public DateTime? MinDate { get; set; }
    public DateTime? MaxDate { get; set; }
    public decimal? MinTotalAmount { get; set; }
    public decimal? MaxTotalAmount { get; set; }
}

public sealed record PaginatedSaleResult(
    IReadOnlyCollection<SaleResult> Items,
    int CurrentPage,
    int PageSize,
    int TotalCount,
    int TotalPages);

public sealed class ListSalesValidator : AbstractValidator<ListSalesQuery>
{
    private static readonly HashSet<string> SupportedOrderFields =
    [
        "salenumber",
        "date",
        "customername",
        "branchname",
        "totalamount",
        "iscancelled"
    ];

    public ListSalesValidator()
    {
        RuleFor(query => query.Page).GreaterThanOrEqualTo(1);
        RuleFor(query => query.Size).InclusiveBetween(1, 100);
        RuleFor(query => query)
            .Must(query => !query.MinDate.HasValue ||
                !query.MaxDate.HasValue ||
                query.MinDate <= query.MaxDate)
            .WithMessage("Minimum date cannot be greater than maximum date.");
        RuleFor(query => query)
            .Must(query => !query.MinTotalAmount.HasValue ||
                !query.MaxTotalAmount.HasValue ||
                query.MinTotalAmount <= query.MaxTotalAmount)
            .WithMessage("Minimum total amount cannot be greater than maximum total amount.");
        RuleFor(query => query.Order)
            .Must(BeValidOrder)
            .When(query => !string.IsNullOrWhiteSpace(query.Order))
            .WithMessage("The order expression contains an unsupported field or direction.");
    }

    private static bool BeValidOrder(string? order)
    {
        if (string.IsNullOrWhiteSpace(order))
            return true;

        return order.Trim().Trim('"').Split(',', StringSplitOptions.RemoveEmptyEntries)
            .Select(clause => clause.Trim().Split(' ', StringSplitOptions.RemoveEmptyEntries))
            .All(parts =>
                parts.Length is 1 or 2 &&
                SupportedOrderFields.Contains(parts[0].ToLowerInvariant()) &&
                (parts.Length == 1 ||
                    parts[1].Equals("asc", StringComparison.OrdinalIgnoreCase) ||
                    parts[1].Equals("desc", StringComparison.OrdinalIgnoreCase)));
    }
}
