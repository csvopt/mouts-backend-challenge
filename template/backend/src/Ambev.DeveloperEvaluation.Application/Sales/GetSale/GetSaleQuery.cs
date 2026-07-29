using Ambev.DeveloperEvaluation.Application.Sales.Common;
using FluentValidation;
using MediatR;

namespace Ambev.DeveloperEvaluation.Application.Sales.GetSale;

public sealed record GetSaleQuery(Guid Id) : IRequest<SaleResult>;

public sealed class GetSaleValidator : AbstractValidator<GetSaleQuery>
{
    public GetSaleValidator()
    {
        RuleFor(query => query.Id).NotEmpty();
    }
}
