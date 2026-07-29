using Ambev.DeveloperEvaluation.Application.Sales.Common;
using Ambev.DeveloperEvaluation.Domain;
using Ambev.DeveloperEvaluation.Domain.Entities;
using Ambev.DeveloperEvaluation.Domain.Repositories;
using AutoMapper;
using MediatR;

namespace Ambev.DeveloperEvaluation.Application.Sales.CreateSale;

public sealed class CreateSaleHandler : IRequestHandler<CreateSaleCommand, SaleResult>
{
    private readonly ISaleRepository _saleRepository;
    private readonly IMapper _mapper;

    public CreateSaleHandler(ISaleRepository saleRepository, IMapper mapper)
    {
        _saleRepository = saleRepository;
        _mapper = mapper;
    }

    public async Task<SaleResult> Handle(
        CreateSaleCommand command,
        CancellationToken cancellationToken)
    {
        var existingSale = await _saleRepository.GetByNumberAsync(
            command.SaleNumber,
            cancellationToken);
        if (existingSale is not null)
            throw new DomainException($"Sale number {command.SaleNumber} already exists.");

        var sale = Sale.Create(
            command.SaleNumber,
            NormalizeDate(command.Date),
            command.CustomerId,
            command.CustomerName,
            command.BranchId,
            command.BranchName);

        foreach (var item in command.Items)
            sale.AddItem(item.ProductId, item.ProductName, item.Quantity, item.UnitPrice);

        await _saleRepository.CreateAsync(sale, cancellationToken);
        return _mapper.Map<SaleResult>(sale);
    }

    private static DateTime NormalizeDate(DateTime date)
    {
        return date.Kind == DateTimeKind.Utc ? date : date.ToUniversalTime();
    }
}
