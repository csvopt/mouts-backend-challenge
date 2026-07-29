using Ambev.DeveloperEvaluation.Domain.Entities;

namespace Ambev.DeveloperEvaluation.Domain.Repositories;

public interface ISaleRepository
{
    Task<Sale> CreateAsync(Sale sale, CancellationToken cancellationToken = default);
    Task<Sale?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<Sale?> GetByNumberAsync(string saleNumber, CancellationToken cancellationToken = default);
    Task<SaleSearchResult> SearchAsync(
        SaleSearchCriteria criteria,
        CancellationToken cancellationToken = default);
    Task UpdateAsync(Sale sale, CancellationToken cancellationToken = default);
    Task<bool> DeleteAsync(Guid id, CancellationToken cancellationToken = default);
}

public sealed record SaleSearchCriteria(
    int Page,
    int Size,
    string? Order = null,
    string? SaleNumber = null,
    string? CustomerName = null,
    string? BranchName = null,
    bool? IsCancelled = null,
    DateTime? MinDate = null,
    DateTime? MaxDate = null,
    decimal? MinTotalAmount = null,
    decimal? MaxTotalAmount = null);

public sealed record SaleSearchResult(
    IReadOnlyCollection<Sale> Items,
    int TotalCount);
