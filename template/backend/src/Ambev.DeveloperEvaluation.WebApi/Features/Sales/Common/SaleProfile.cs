using Ambev.DeveloperEvaluation.Application.Sales.Common;
using Ambev.DeveloperEvaluation.Application.Sales.CreateSale;
using Ambev.DeveloperEvaluation.Application.Sales.UpdateSale;
using AutoMapper;

namespace Ambev.DeveloperEvaluation.WebApi.Features.Sales.Common;

public sealed class SaleProfile : Profile
{
    public SaleProfile()
    {
        CreateMap<SaleItemRequest, SaleItemCommand>();
        CreateMap<SaleRequest, CreateSaleCommand>();
        CreateMap<SaleRequest, UpdateSaleCommand>()
            .ForMember(command => command.Id, options => options.Ignore());

        CreateMap<SaleResult, SaleResponse>();
        CreateMap<SaleItemResult, SaleItemResponse>();
    }
}
