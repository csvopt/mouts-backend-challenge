using Ambev.DeveloperEvaluation.Application.Sales.Common;
using FluentValidation;
using MediatR;

namespace Ambev.DeveloperEvaluation.Application.Sales.CancelSale;

public sealed record CancelSaleCommand(Guid Id) : IRequest<SaleResult>;

public sealed class CancelSaleValidator : AbstractValidator<CancelSaleCommand>
{
    public CancelSaleValidator()
    {
        RuleFor(command => command.Id).NotEmpty();
    }
}
