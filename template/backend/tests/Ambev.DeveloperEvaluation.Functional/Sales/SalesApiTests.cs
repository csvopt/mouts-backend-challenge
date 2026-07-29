using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using Ambev.DeveloperEvaluation.Functional.Infrastructure;
using FluentAssertions;
using Xunit;

namespace Ambev.DeveloperEvaluation.Functional.Sales;

public sealed class SalesApiTests
    : IClassFixture<DeveloperEvaluationWebApplicationFactory>
{
    private readonly HttpClient _client;

    public SalesApiTests(DeveloperEvaluationWebApplicationFactory factory)
    {
        _client = factory.CreateClient();
    }

    [Fact(DisplayName = "Given the sales API When executing its lifecycle Then returns consistent data")]
    public async Task SalesLifecycle_ReturnsExpectedResponses()
    {
        // Given
        var productId = Guid.NewGuid();
        var request = CreateRequest(productId, quantity: 4);

        // When - create
        var createResponse = await _client.PostAsJsonAsync("/api/sales", request);

        // Then - calculated response
        createResponse.StatusCode.Should().Be(HttpStatusCode.Created);
        using var createdDocument = await ReadDocument(createResponse);
        var createdSale = createdDocument.RootElement.GetProperty("data");
        var saleId = createdSale.GetProperty("id").GetGuid();
        var itemId = createdSale.GetProperty("items")[0].GetProperty("id").GetGuid();
        createdSale.GetProperty("totalAmount").GetDecimal().Should().Be(360m);
        createdSale.GetProperty("items")[0]
            .GetProperty("discountPercentage").GetDecimal().Should().Be(0.10m);

        // When - retrieve and filter
        var getResponse = await _client.GetAsync($"/api/sales/{saleId}");
        var listResponse = await _client.GetAsync(
            "/api/sales?_page=1&_size=10&_order=date%20desc&customerName=*Customer*");

        // Then
        getResponse.StatusCode.Should().Be(HttpStatusCode.OK);
        listResponse.StatusCode.Should().Be(HttpStatusCode.OK);
        using var listDocument = await ReadDocument(listResponse);
        listDocument.RootElement.GetProperty("totalCount").GetInt32().Should().Be(1);

        // When - cancel item
        var cancelResponse = await _client.PatchAsync(
            $"/api/sales/{saleId}/items/{itemId}/cancel",
            null);

        // Then
        cancelResponse.StatusCode.Should().Be(HttpStatusCode.OK);
        using var cancelledDocument = await ReadDocument(cancelResponse);
        cancelledDocument.RootElement.GetProperty("data")
            .GetProperty("totalAmount").GetDecimal().Should().Be(0m);

        // When - delete
        var deleteResponse = await _client.DeleteAsync($"/api/sales/{saleId}");
        var missingResponse = await _client.GetAsync($"/api/sales/{saleId}");

        // Then
        deleteResponse.StatusCode.Should().Be(HttpStatusCode.NoContent);
        missingResponse.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact(DisplayName = "Given more than twenty identical items When creating a sale Then returns validation error")]
    public async Task CreateSale_QuantityAboveLimit_ReturnsBadRequest()
    {
        // Given
        var request = CreateRequest(Guid.NewGuid(), quantity: 21);

        // When
        var response = await _client.PostAsJsonAsync("/api/sales", request);

        // Then
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        using var document = await ReadDocument(response);
        document.RootElement.GetProperty("success").GetBoolean().Should().BeFalse();
        document.RootElement.GetProperty("errors").GetArrayLength().Should().BeGreaterThan(0);
    }

    private static object CreateRequest(Guid productId, int quantity)
    {
        return new
        {
            saleNumber = $"SALE-{Guid.NewGuid():N}",
            date = DateTime.UtcNow,
            customerId = Guid.NewGuid(),
            customerName = "Functional Customer",
            branchId = Guid.NewGuid(),
            branchName = "Functional Branch",
            items = new[]
            {
                new
                {
                    productId,
                    productName = "Functional Product",
                    quantity,
                    unitPrice = 100m
                }
            }
        };
    }

    private static async Task<JsonDocument> ReadDocument(HttpResponseMessage response)
    {
        var content = await response.Content.ReadAsStringAsync();
        return JsonDocument.Parse(content);
    }
}
