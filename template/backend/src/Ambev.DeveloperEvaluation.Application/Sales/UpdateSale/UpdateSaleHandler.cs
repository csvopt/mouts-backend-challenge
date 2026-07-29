using Ambev.DeveloperEvaluation.Application.Common.Messaging;
using Ambev.DeveloperEvaluation.Application.Sales.Common;
using Ambev.DeveloperEvaluation.Domain;
using Ambev.DeveloperEvaluation.Domain.Entities;
using Ambev.DeveloperEvaluation.Domain.Events;
using Ambev.DeveloperEvaluation.Domain.Repositories;
using AutoMapper;
using MediatR;

namespace Ambev.DeveloperEvaluation.Application.Sales.UpdateSale;

public sealed class UpdateSaleHandler : IRequestHandler<UpdateSaleCommand, SaleResult>
{
    private readonly ISaleRepository _saleRepository;
    private readonly IMapper _mapper;
    private readonly IEventPublisher _eventPublisher;

    public UpdateSaleHandler(
        ISaleRepository saleRepository,
        IMapper mapper,
        IEventPublisher eventPublisher)
    {
        _saleRepository = saleRepository;
        _mapper = mapper;
        _eventPublisher = eventPublisher;
    }

    public async Task<SaleResult> Handle(
        UpdateSaleCommand command,
        CancellationToken cancellationToken)
    {
        var sale = await _saleRepository.GetByIdAsync(command.Id, cancellationToken)
            ?? throw new KeyNotFoundException($"Sale with ID {command.Id} was not found.");

        var saleWithSameNumber = await _saleRepository.GetByNumberAsync(
            command.SaleNumber,
            cancellationToken);
        if (saleWithSameNumber is not null && saleWithSameNumber.Id != command.Id)
            throw new DomainException($"Sale number {command.SaleNumber} already exists.");

        sale.UpdateDetails(
            command.SaleNumber,
            NormalizeDate(command.Date),
            command.CustomerId,
            command.CustomerName,
            command.BranchId,
            command.BranchName);
        sale.ReplaceItems(command.Items.Select(item => new SaleItemData(
            item.ProductId,
            item.ProductName,
            item.Quantity,
            item.UnitPrice)));

        await _saleRepository.UpdateAsync(sale, cancellationToken);
        await _eventPublisher.PublishAsync(
            new SaleModifiedEvent(sale.Id, sale.SaleNumber, DateTime.UtcNow),
            cancellationToken);
        return _mapper.Map<SaleResult>(sale);
    }

    private static DateTime NormalizeDate(DateTime date)
    {
        return date.Kind == DateTimeKind.Utc ? date : date.ToUniversalTime();
    }
}
