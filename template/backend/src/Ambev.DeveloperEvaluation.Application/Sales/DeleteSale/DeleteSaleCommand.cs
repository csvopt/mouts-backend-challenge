using FluentValidation;
using MediatR;

namespace Ambev.DeveloperEvaluation.Application.Sales.DeleteSale;

public sealed record DeleteSaleCommand(Guid Id) : IRequest;

public sealed class DeleteSaleValidator : AbstractValidator<DeleteSaleCommand>
{
    public DeleteSaleValidator()
    {
        RuleFor(command => command.Id).NotEmpty();
    }
}
