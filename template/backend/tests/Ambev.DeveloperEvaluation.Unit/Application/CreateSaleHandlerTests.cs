using Ambev.DeveloperEvaluation.Application.Common.Messaging;
using Ambev.DeveloperEvaluation.Application.Sales.Common;
using Ambev.DeveloperEvaluation.Application.Sales.CreateSale;
using Ambev.DeveloperEvaluation.Domain;
using Ambev.DeveloperEvaluation.Domain.Entities;
using Ambev.DeveloperEvaluation.Domain.Events;
using Ambev.DeveloperEvaluation.Domain.Repositories;
using Ambev.DeveloperEvaluation.Unit.Application.TestData;
using AutoMapper;
using FluentAssertions;
using NSubstitute;
using Xunit;

namespace Ambev.DeveloperEvaluation.Unit.Application;

public sealed class CreateSaleHandlerTests
{
    private readonly ISaleRepository _saleRepository = Substitute.For<ISaleRepository>();
    private readonly IMapper _mapper = Substitute.For<IMapper>();
    private readonly IEventPublisher _eventPublisher = Substitute.For<IEventPublisher>();

    [Fact(DisplayName = "Given valid sale data When creating it Then persists calculated sale and publishes event")]
    public async Task Handle_ValidCommand_PersistsSaleAndPublishesEvent()
    {
        // Given
        var command = SaleCommandTestData.CreateValidCommand(quantity: 10);
        Sale? persistedSale = null;
        _saleRepository
            .CreateAsync(Arg.Do<Sale>(sale => persistedSale = sale), Arg.Any<CancellationToken>())
            .Returns(callInfo => callInfo.Arg<Sale>());
        _mapper.Map<SaleResult>(Arg.Any<Sale>())
            .Returns(callInfo => new SaleResult { Id = callInfo.Arg<Sale>().Id });
        var handler = CreateHandler();

        // When
        var result = await handler.Handle(command, CancellationToken.None);

        // Then
        result.Id.Should().NotBeEmpty();
        persistedSale.Should().NotBeNull();
        persistedSale!.TotalAmount.Should().Be(800m);
        persistedSale.Items.Single().DiscountPercentage.Should().Be(0.20m);
        await _saleRepository.Received(1)
            .CreateAsync(persistedSale, Arg.Any<CancellationToken>());
        await _eventPublisher.Received(1)
            .PublishAsync(
                Arg.Is<SaleCreatedEvent>(domainEvent =>
                    domainEvent.SaleId == persistedSale.Id &&
                    domainEvent.SaleNumber == persistedSale.SaleNumber),
                Arg.Any<CancellationToken>());
    }

    [Fact(DisplayName = "Given an existing sale number When creating a sale Then rejects the conflict")]
    public async Task Handle_DuplicatedSaleNumber_ThrowsDomainException()
    {
        // Given
        var command = SaleCommandTestData.CreateValidCommand();
        var existingSale = Sale.Create(
            command.SaleNumber,
            command.Date,
            command.CustomerId,
            command.CustomerName,
            command.BranchId,
            command.BranchName);
        _saleRepository.GetByNumberAsync(command.SaleNumber, Arg.Any<CancellationToken>())
            .Returns(existingSale);
        var handler = CreateHandler();

        // When
        var act = () => handler.Handle(command, CancellationToken.None);

        // Then
        await act.Should().ThrowAsync<DomainException>()
            .WithMessage("*already exists*");
        await _saleRepository.DidNotReceive()
            .CreateAsync(Arg.Any<Sale>(), Arg.Any<CancellationToken>());
        await _eventPublisher.DidNotReceive()
            .PublishAsync(Arg.Any<SaleCreatedEvent>(), Arg.Any<CancellationToken>());
    }

    private CreateSaleHandler CreateHandler()
    {
        return new CreateSaleHandler(_saleRepository, _mapper, _eventPublisher);
    }
}
