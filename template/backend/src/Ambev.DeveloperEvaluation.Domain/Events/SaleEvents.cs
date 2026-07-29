namespace Ambev.DeveloperEvaluation.Domain.Events;

public sealed record SaleCreatedEvent(
    Guid SaleId,
    string SaleNumber,
    DateTime OccurredAt);

public sealed record SaleModifiedEvent(
    Guid SaleId,
    string SaleNumber,
    DateTime OccurredAt);

public sealed record SaleCancelledEvent(
    Guid SaleId,
    string SaleNumber,
    DateTime OccurredAt);

public sealed record ItemCancelledEvent(
    Guid SaleId,
    Guid ItemId,
    Guid ProductId,
    DateTime OccurredAt);
