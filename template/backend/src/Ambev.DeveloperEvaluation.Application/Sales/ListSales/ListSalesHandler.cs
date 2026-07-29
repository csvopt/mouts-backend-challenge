using Ambev.DeveloperEvaluation.Application.Sales.Common;
using Ambev.DeveloperEvaluation.Domain.Repositories;
using AutoMapper;
using MediatR;

namespace Ambev.DeveloperEvaluation.Application.Sales.ListSales;

public sealed class ListSalesHandler : IRequestHandler<ListSalesQuery, PaginatedSaleResult>
{
    private readonly ISaleRepository _saleRepository;
    private readonly IMapper _mapper;

    public ListSalesHandler(ISaleRepository saleRepository, IMapper mapper)
    {
        _saleRepository = saleRepository;
        _mapper = mapper;
    }

    public async Task<PaginatedSaleResult> Handle(
        ListSalesQuery query,
        CancellationToken cancellationToken)
    {
        var criteria = new SaleSearchCriteria(
            query.Page,
            query.Size,
            TrimQuotes(query.Order),
            query.SaleNumber,
            query.CustomerName,
            query.BranchName,
            query.IsCancelled,
            NormalizeDate(query.MinDate),
            NormalizeDate(query.MaxDate),
            query.MinTotalAmount,
            query.MaxTotalAmount);

        var result = await _saleRepository.SearchAsync(criteria, cancellationToken);
        var totalPages = (int)Math.Ceiling(result.TotalCount / (double)query.Size);

        return new PaginatedSaleResult(
            _mapper.Map<IReadOnlyCollection<SaleResult>>(result.Items),
            query.Page,
            query.Size,
            result.TotalCount,
            totalPages);
    }

    private static string? TrimQuotes(string? value)
    {
        return value?.Trim().Trim('"');
    }

    private static DateTime? NormalizeDate(DateTime? date)
    {
        return date.HasValue && date.Value.Kind != DateTimeKind.Utc
            ? date.Value.ToUniversalTime()
            : date;
    }
}
