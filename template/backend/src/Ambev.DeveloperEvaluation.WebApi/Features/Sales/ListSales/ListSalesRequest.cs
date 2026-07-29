using Ambev.DeveloperEvaluation.Application.Sales.ListSales;
using AutoMapper;

namespace Ambev.DeveloperEvaluation.WebApi.Features.Sales.ListSales;

public sealed class ListSalesRequest
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

public sealed class ListSalesProfile : Profile
{
    public ListSalesProfile()
    {
        CreateMap<ListSalesRequest, ListSalesQuery>();
    }
}
