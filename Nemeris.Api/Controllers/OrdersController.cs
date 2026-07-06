using System.Security.Claims;
using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Localization;
using Nemeris.Api.Contracts;
using Nemeris.Api.Extensions;
using Nemeris.Core.Dtos;
using Nemeris.Core.Enums;
using Nemeris.Core.Interfaces;
using Nemeris.Infrastructure.Data;
using Nemeris.Shared.Common;
using Nemeris.Shared.Orders;

namespace Nemeris.Api.Controllers;

[ApiController]
[Route("api/orders")]
[Authorize]
public class OrdersController(
    IOrderService orderService,
    IEmailService emailService,
    IMapper mapper,
    IStringLocalizer<SharedResources> localizer,
    ILogger<OrdersController> logger) : ControllerBase
{
    [HttpGet]
    public async Task<ActionResult<PagedResult<OrderDto>>> GetMyOrders(
        [FromQuery] int page = 1, [FromQuery] int pageSize = 20, CancellationToken ct = default)
    {
        var orders = await orderService.GetForUserAsync(User.GetUserId(), page, pageSize, ct);
        return Ok(mapper.Map<PagedResult<OrderDto>>(orders));
    }

    [HttpGet("{id:guid}")]
    public async Task<ActionResult<OrderDto>> GetOrder(Guid id, CancellationToken ct)
    {
        // Admins see any order; customers only their own (scoping is in the query itself).
        var scopeToUser = User.IsInRole(DbSeeder.AdminRole) ? (Guid?)null : User.GetUserId();
        var order = await orderService.GetByIdAsync(id, scopeToUser, ct);
        return order is null ? NotFound() : Ok(mapper.Map<OrderDto>(order));
    }

    [HttpPost]
    public async Task<ActionResult<OrderDto>> Checkout(CheckoutRequest request, CancellationToken ct)
    {
        var order = await orderService.CheckoutAsync(
            User.GetUserId(),
            new CheckoutDto { ShippingAddressId = request.ShippingAddressId, Notes = request.Notes },
            ct);

        // Confirmation email is best-effort: the order is already committed, so a
        // mail outage must not turn a successful checkout into an error response.
        var email = User.FindFirstValue(ClaimTypes.Email);
        if (!string.IsNullOrEmpty(email))
        {
            try
            {
                await emailService.SendAsync(
                    email,
                    string.Format(localizer["Email.OrderConfirmationSubject"], order.OrderNumber),
                    string.Format(localizer["Email.OrderConfirmationBody"], order.OrderNumber, order.Total),
                    ct);
            }
            catch (Exception ex)
            {
                logger.LogWarning(ex, "Order confirmation email for {OrderNumber} failed.", order.OrderNumber);
            }
        }

        return CreatedAtAction(nameof(GetOrder), new { id = order.Id }, mapper.Map<OrderDto>(order));
    }

    [Authorize(Roles = DbSeeder.AdminRole)]
    [HttpGet("all")]
    public async Task<ActionResult<PagedResult<OrderDto>>> GetAllOrders(
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20,
        [FromQuery] OrderStatus? status = null,
        CancellationToken ct = default)
    {
        var orders = await orderService.GetPagedAsync(page, pageSize, status, ct);
        return Ok(mapper.Map<PagedResult<OrderDto>>(orders));
    }

    [Authorize(Roles = DbSeeder.AdminRole)]
    [HttpPut("{id:guid}/status")]
    public async Task<ActionResult<OrderDto>> UpdateStatus(Guid id, UpdateOrderStatusRequest request, CancellationToken ct)
    {
        var order = await orderService.UpdateStatusAsync(id, request.Status, ct);
        return order is null ? NotFound() : Ok(mapper.Map<OrderDto>(order));
    }
}
