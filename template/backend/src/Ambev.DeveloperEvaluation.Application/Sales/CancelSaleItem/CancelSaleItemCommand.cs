using Ambev.DeveloperEvaluation.Application.Sales.Common;
using FluentValidation;
using MediatR;

namespace Ambev.DeveloperEvaluation.Application.Sales.CancelSaleItem;

public sealed record CancelSaleItemCommand(Guid SaleId, Guid ItemId) : IRequest<SaleResult>;

public sealed class CancelSaleItemValidator : AbstractValidator<CancelSaleItemCommand>
{
    public CancelSaleItemValidator()
    {
        RuleFor(command => command.SaleId).NotEmpty();
        RuleFor(command => command.ItemId).NotEmpty();
    }
}
