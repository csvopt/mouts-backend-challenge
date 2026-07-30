using Ambev.DeveloperEvaluation.Application.Common.Messaging;
using Ambev.DeveloperEvaluation.Application.Sales.Common;
using Ambev.DeveloperEvaluation.Domain.Events;
using Ambev.DeveloperEvaluation.Domain.Repositories;
using AutoMapper;
using MediatR;

namespace Ambev.DeveloperEvaluation.Application.Sales.CancelSaleItem;

public sealed class CancelSaleItemHandler : IRequestHandler<CancelSaleItemCommand, SaleResult>
{
    private readonly ISaleRepository _saleRepository;
    private readonly IMapper _mapper;
    private readonly IEventPublisher _eventPublisher;

    public CancelSaleItemHandler(
        ISaleRepository saleRepository,
        IMapper mapper,
        IEventPublisher eventPublisher)
    {
        _saleRepository = saleRepository;
        _mapper = mapper;
        _eventPublisher = eventPublisher;
    }

    public async Task<SaleResult> Handle(
        CancelSaleItemCommand command,
        CancellationToken cancellationToken)
    {
        var sale = await _saleRepository.GetByIdAsync(command.SaleId, cancellationToken)
            ?? throw new KeyNotFoundException($"Sale with ID {command.SaleId} was not found.");

        var item = sale.Items.FirstOrDefault(item => item.Id == command.ItemId)
            ?? throw new KeyNotFoundException(
                $"Item {command.ItemId} was not found in sale {command.SaleId}.");

        sale.CancelItem(item.Id);
        await _saleRepository.UpdateAsync(sale, cancellationToken);
        await _eventPublisher.PublishAsync(
            new ItemCancelledEvent(sale.Id, item.Id, item.ProductId, DateTime.UtcNow),
            cancellationToken);

        return _mapper.Map<SaleResult>(sale);
    }
}
