using Ambev.DeveloperEvaluation.Application.Sales.CancelSale;
using Ambev.DeveloperEvaluation.Application.Sales.CancelSaleItem;
using Ambev.DeveloperEvaluation.Application.Sales.CreateSale;
using Ambev.DeveloperEvaluation.Application.Sales.DeleteSale;
using Ambev.DeveloperEvaluation.Application.Sales.GetSale;
using Ambev.DeveloperEvaluation.Application.Sales.ListSales;
using Ambev.DeveloperEvaluation.Application.Sales.UpdateSale;
using Ambev.DeveloperEvaluation.WebApi.Common;
using Ambev.DeveloperEvaluation.WebApi.Features.Sales.Common;
using Ambev.DeveloperEvaluation.WebApi.Features.Sales.ListSales;
using AutoMapper;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace Ambev.DeveloperEvaluation.WebApi.Features.Sales;

[ApiController]
[Route("api/[controller]")]
public sealed class SalesController : ControllerBase
{
    private readonly IMediator _mediator;
    private readonly IMapper _mapper;

    public SalesController(IMediator mediator, IMapper mapper)
    {
        _mediator = mediator;
        _mapper = mapper;
    }

    [HttpPost]
    [ProducesResponseType(typeof(ApiResponseWithData<SaleResponse>), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status409Conflict)]
    public async Task<IActionResult> Create(
        [FromBody] SaleRequest request,
        CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(
            _mapper.Map<CreateSaleCommand>(request),
            cancellationToken);
        var response = _mapper.Map<SaleResponse>(result);

        return CreatedAtAction(
            nameof(GetById),
            new { id = response.Id },
            Success(response, "Sale created successfully."));
    }

    [HttpGet("{id:guid}")]
    [ProducesResponseType(typeof(ApiResponseWithData<SaleResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetById(Guid id, CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(new GetSaleQuery(id), cancellationToken);
        return Ok(Success(_mapper.Map<SaleResponse>(result), "Sale retrieved successfully."));
    }

    [HttpGet]
    [ProducesResponseType(typeof(PaginatedResponse<SaleResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> List(
        [FromQuery(Name = "_page")] int page = 1,
        [FromQuery(Name = "_size")] int size = 10,
        [FromQuery(Name = "_order")] string? order = null,
        [FromQuery] string? saleNumber = null,
        [FromQuery] string? customerName = null,
        [FromQuery] string? branchName = null,
        [FromQuery] bool? isCancelled = null,
        [FromQuery(Name = "_minDate")] DateTime? minDate = null,
        [FromQuery(Name = "_maxDate")] DateTime? maxDate = null,
        [FromQuery(Name = "_minTotalAmount")] decimal? minTotalAmount = null,
        [FromQuery(Name = "_maxTotalAmount")] decimal? maxTotalAmount = null,
        CancellationToken cancellationToken = default)
    {
        var request = new ListSalesRequest
        {
            Page = page,
            Size = size,
            Order = order,
            SaleNumber = saleNumber,
            CustomerName = customerName,
            BranchName = branchName,
            IsCancelled = isCancelled,
            MinDate = minDate,
            MaxDate = maxDate,
            MinTotalAmount = minTotalAmount,
            MaxTotalAmount = maxTotalAmount
        };

        var result = await _mediator.Send(
            _mapper.Map<ListSalesQuery>(request),
            cancellationToken);

        return Ok(new PaginatedResponse<SaleResponse>
        {
            Success = true,
            Message = "Sales retrieved successfully.",
            Data = _mapper.Map<IReadOnlyCollection<SaleResponse>>(result.Items),
            CurrentPage = result.CurrentPage,
            PageSize = result.PageSize,
            TotalPages = result.TotalPages,
            TotalCount = result.TotalCount
        });
    }

    [HttpPut("{id:guid}")]
    [ProducesResponseType(typeof(ApiResponseWithData<SaleResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status409Conflict)]
    public async Task<IActionResult> Update(
        Guid id,
        [FromBody] SaleRequest request,
        CancellationToken cancellationToken)
    {
        var command = _mapper.Map<UpdateSaleCommand>(request);
        command.Id = id;

        var result = await _mediator.Send(command, cancellationToken);
        return Ok(Success(_mapper.Map<SaleResponse>(result), "Sale updated successfully."));
    }

    [HttpDelete("{id:guid}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Delete(Guid id, CancellationToken cancellationToken)
    {
        await _mediator.Send(new DeleteSaleCommand(id), cancellationToken);
        return NoContent();
    }

    [HttpPatch("{id:guid}/cancel")]
    [ProducesResponseType(typeof(ApiResponseWithData<SaleResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status409Conflict)]
    public async Task<IActionResult> Cancel(Guid id, CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(new CancelSaleCommand(id), cancellationToken);
        return Ok(Success(_mapper.Map<SaleResponse>(result), "Sale cancelled successfully."));
    }

    [HttpPatch("{saleId:guid}/items/{itemId:guid}/cancel")]
    [ProducesResponseType(typeof(ApiResponseWithData<SaleResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status409Conflict)]
    public async Task<IActionResult> CancelItem(
        Guid saleId,
        Guid itemId,
        CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(
            new CancelSaleItemCommand(saleId, itemId),
            cancellationToken);
        return Ok(Success(_mapper.Map<SaleResponse>(result), "Sale item cancelled successfully."));
    }

    private static ApiResponseWithData<T> Success<T>(T data, string message)
    {
        return new ApiResponseWithData<T>
        {
            Success = true,
            Message = message,
            Data = data
        };
    }
}
