using System.Linq.Expressions;
using Ambev.DeveloperEvaluation.Domain.Entities;
using Ambev.DeveloperEvaluation.Domain.Repositories;
using Microsoft.EntityFrameworkCore;

namespace Ambev.DeveloperEvaluation.ORM.Repositories;

public class SaleRepository : ISaleRepository
{
    private readonly DefaultContext _context;

    public SaleRepository(DefaultContext context)
    {
        _context = context;
    }

    public async Task<Sale> CreateAsync(Sale sale, CancellationToken cancellationToken = default)
    {
        await _context.Sales.AddAsync(sale, cancellationToken);
        await _context.SaveChangesAsync(cancellationToken);
        return sale;
    }

    public Task<Sale?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return _context.Sales
            .Include(sale => sale.Items)
            .FirstOrDefaultAsync(sale => sale.Id == id, cancellationToken);
    }

    public Task<Sale?> GetByNumberAsync(
        string saleNumber,
        CancellationToken cancellationToken = default)
    {
        return _context.Sales
            .Include(sale => sale.Items)
            .FirstOrDefaultAsync(sale => sale.SaleNumber == saleNumber, cancellationToken);
    }

    public async Task<SaleSearchResult> SearchAsync(
        SaleSearchCriteria criteria,
        CancellationToken cancellationToken = default)
    {
        IQueryable<Sale> query = _context.Sales
            .AsNoTracking()
            .Include(sale => sale.Items);

        query = ApplyFilters(query, criteria);
        query = ApplyOrdering(query, criteria.Order);

        var totalCount = await query.CountAsync(cancellationToken);
        var items = await query
            .Skip((criteria.Page - 1) * criteria.Size)
            .Take(criteria.Size)
            .ToListAsync(cancellationToken);

        return new SaleSearchResult(items, totalCount);
    }

    public async Task UpdateAsync(Sale sale, CancellationToken cancellationToken = default)
    {
        _context.Sales.Update(sale);
        await _context.SaveChangesAsync(cancellationToken);
    }

    public async Task<bool> DeleteAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var sale = await GetByIdAsync(id, cancellationToken);
        if (sale is null)
            return false;

        _context.Sales.Remove(sale);
        await _context.SaveChangesAsync(cancellationToken);
        return true;
    }

    private static IQueryable<Sale> ApplyFilters(
        IQueryable<Sale> query,
        SaleSearchCriteria criteria)
    {
        if (!string.IsNullOrWhiteSpace(criteria.SaleNumber))
            query = ApplyStringFilter(query, sale => sale.SaleNumber, criteria.SaleNumber);

        if (!string.IsNullOrWhiteSpace(criteria.CustomerName))
            query = ApplyStringFilter(query, sale => sale.CustomerName, criteria.CustomerName);

        if (!string.IsNullOrWhiteSpace(criteria.BranchName))
            query = ApplyStringFilter(query, sale => sale.BranchName, criteria.BranchName);

        if (criteria.IsCancelled.HasValue)
            query = query.Where(sale => sale.IsCancelled == criteria.IsCancelled.Value);

        if (criteria.MinDate.HasValue)
            query = query.Where(sale => sale.Date >= criteria.MinDate.Value);

        if (criteria.MaxDate.HasValue)
            query = query.Where(sale => sale.Date <= criteria.MaxDate.Value);

        if (criteria.MinTotalAmount.HasValue)
            query = query.Where(sale => sale.TotalAmount >= criteria.MinTotalAmount.Value);

        if (criteria.MaxTotalAmount.HasValue)
            query = query.Where(sale => sale.TotalAmount <= criteria.MaxTotalAmount.Value);

        return query;
    }

    private static IQueryable<Sale> ApplyStringFilter(
        IQueryable<Sale> query,
        Expression<Func<Sale, string>> property,
        string value)
    {
        if (!value.Contains('*'))
            return query.Where(BuildEqualityExpression(property, value));

        var startsWithWildcard = value.StartsWith('*');
        var endsWithWildcard = value.EndsWith('*');
        var searchValue = value.Trim('*');

        var methodName = (startsWithWildcard, endsWithWildcard) switch
        {
            (true, true) => nameof(string.Contains),
            (true, false) => nameof(string.EndsWith),
            _ => nameof(string.StartsWith)
        };

        return query.Where(BuildStringMethodExpression(property, methodName, searchValue));
    }

    private static Expression<Func<Sale, bool>> BuildEqualityExpression(
        Expression<Func<Sale, string>> property,
        string value)
    {
        var equals = Expression.Equal(property.Body, Expression.Constant(value));
        return Expression.Lambda<Func<Sale, bool>>(equals, property.Parameters);
    }

    private static Expression<Func<Sale, bool>> BuildStringMethodExpression(
        Expression<Func<Sale, string>> property,
        string methodName,
        string value)
    {
        var stringMethod = typeof(string).GetMethod(methodName, [typeof(string)])!;
        var methodCall = Expression.Call(
            property.Body,
            stringMethod,
            Expression.Constant(value));

        return Expression.Lambda<Func<Sale, bool>>(methodCall, property.Parameters);
    }

    private static IQueryable<Sale> ApplyOrdering(IQueryable<Sale> query, string? order)
    {
        if (string.IsNullOrWhiteSpace(order))
            return query.OrderBy(sale => sale.SaleNumber);

        IOrderedQueryable<Sale>? orderedQuery = null;
        foreach (var orderClause in order.Split(',', StringSplitOptions.RemoveEmptyEntries))
        {
            var parts = orderClause.Trim().Split(' ', StringSplitOptions.RemoveEmptyEntries);
            var field = parts[0].ToLowerInvariant();
            var descending = parts.Length > 1 &&
                parts[1].Equals("desc", StringComparison.OrdinalIgnoreCase);

            orderedQuery = ApplyOrderClause(orderedQuery ?? query, orderedQuery is not null, field, descending);
        }

        return orderedQuery ?? query.OrderBy(sale => sale.SaleNumber);
    }

    private static IOrderedQueryable<Sale> ApplyOrderClause(
        IQueryable<Sale> query,
        bool useThenBy,
        string field,
        bool descending)
    {
        return field switch
        {
            "salenumber" => Order(query, sale => sale.SaleNumber, useThenBy, descending),
            "date" => Order(query, sale => sale.Date, useThenBy, descending),
            "customername" => Order(query, sale => sale.CustomerName, useThenBy, descending),
            "branchname" => Order(query, sale => sale.BranchName, useThenBy, descending),
            "totalamount" => Order(query, sale => sale.TotalAmount, useThenBy, descending),
            "iscancelled" => Order(query, sale => sale.IsCancelled, useThenBy, descending),
            _ => throw new ArgumentException($"Ordering by '{field}' is not supported.")
        };
    }

    private static IOrderedQueryable<Sale> Order<TKey>(
        IQueryable<Sale> query,
        Expression<Func<Sale, TKey>> keySelector,
        bool useThenBy,
        bool descending)
    {
        if (!useThenBy)
            return descending ? query.OrderByDescending(keySelector) : query.OrderBy(keySelector);

        var orderedQuery = (IOrderedQueryable<Sale>)query;
        return descending
            ? orderedQuery.ThenByDescending(keySelector)
            : orderedQuery.ThenBy(keySelector);
    }
}
