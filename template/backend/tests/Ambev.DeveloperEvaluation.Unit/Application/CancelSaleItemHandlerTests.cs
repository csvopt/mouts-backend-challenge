using Ambev.DeveloperEvaluation.Application.Common.Messaging;
using Ambev.DeveloperEvaluation.Application.Sales.CancelSaleItem;
using Ambev.DeveloperEvaluation.Application.Sales.Common;
using Ambev.DeveloperEvaluation.Domain.Entities;
using Ambev.DeveloperEvaluation.Domain.Events;
using Ambev.DeveloperEvaluation.Domain.Repositories;
using Ambev.DeveloperEvaluation.Unit.Domain.Entities.TestData;
using AutoMapper;
using FluentAssertions;
using NSubstitute;
using Xunit;

namespace Ambev.DeveloperEvaluation.Unit.Application;

public sealed class CancelSaleItemHandlerTests
{
    [Fact(DisplayName = "Given an active sale item When cancelling it Then updates total and publishes event")]
    public async Task Handle_ActiveItem_CancelsItemAndPublishesEvent()
    {
        // Given
        var sale = SaleTestData.CreateSale();
        var item = SaleTestData.AddItem(sale, quantity: 4, unitPrice: 10m);
        SaleTestData.AddItem(sale, quantity: 2, unitPrice: 25m);
        var repository = Substitute.For<ISaleRepository>();
        var mapper = Substitute.For<IMapper>();
        var publisher = Substitute.For<IEventPublisher>();
        repository.GetByIdAsync(sale.Id, Arg.Any<CancellationToken>()).Returns(sale);
        mapper.Map<SaleResult>(sale).Returns(new SaleResult
        {
            Id = sale.Id,
            TotalAmount = 50m
        });
        var handler = new CancelSaleItemHandler(repository, mapper, publisher);

        // When
        var result = await handler.Handle(
            new CancelSaleItemCommand(sale.Id, item.Id),
            CancellationToken.None);

        // Then
        item.IsCancelled.Should().BeTrue();
        sale.TotalAmount.Should().Be(50m);
        result.TotalAmount.Should().Be(50m);
        await repository.Received(1).UpdateAsync(sale, Arg.Any<CancellationToken>());
        await publisher.Received(1).PublishAsync(
            Arg.Is<ItemCancelledEvent>(domainEvent =>
                domainEvent.SaleId == sale.Id &&
                domainEvent.ItemId == item.Id &&
                domainEvent.ProductId == item.ProductId),
            Arg.Any<CancellationToken>());
    }

    [Fact(DisplayName = "Given an unknown item When cancelling it Then returns not found")]
    public async Task Handle_UnknownItem_ThrowsKeyNotFoundException()
    {
        // Given
        var sale = SaleTestData.CreateSale();
        SaleTestData.AddItem(sale);
        var repository = Substitute.For<ISaleRepository>();
        repository.GetByIdAsync(sale.Id, Arg.Any<CancellationToken>()).Returns(sale);
        var handler = new CancelSaleItemHandler(
            repository,
            Substitute.For<IMapper>(),
            Substitute.For<IEventPublisher>());

        // When
        var act = () => handler.Handle(
            new CancelSaleItemCommand(sale.Id, Guid.NewGuid()),
            CancellationToken.None);

        // Then
        await act.Should().ThrowAsync<KeyNotFoundException>();
    }
}
