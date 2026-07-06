using AutoMapper;
using FluentValidation;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Localization;
using Nemeris.Api.Extensions;
using Nemeris.Core.Dtos;
using Nemeris.Core.Interfaces;
using Nemeris.Shared.Cart;

namespace Nemeris.Api.Controllers;

[ApiController]
[Route("api/cart")]
[Authorize]
public class CartController(
    ICartService cartService,
    IMapper mapper,
    IValidator<CartItemUpsertDto> validator,
    IStringLocalizer<SharedResources> localizer) : ControllerBase
{
    [HttpGet]
    public async Task<ActionResult<List<CartItemDto>>> GetCart(CancellationToken ct)
    {
        var items = await cartService.GetItemsAsync(User.GetUserId(), ct);
        return Ok(mapper.Map<List<CartItemDto>>(items));
    }

    /// <summary>
    /// Idempotent add-or-set: PUT because replaying the same request (e.g. the App's
    /// offline queue syncing twice) must land on the same state. Quantity 0 removes.
    /// </summary>
    [HttpPut("items")]
    public async Task<ActionResult<CartItemDto>> UpsertItem(UpsertCartItemRequest request, CancellationToken ct)
    {
        var dto = new CartItemUpsertDto { ProductId = request.ProductId, Quantity = request.Quantity };

        var validation = await validator.ValidateAsync(dto, ct);
        if (!validation.IsValid)
        {
            return ValidationProblem(new ValidationProblemDetails(validation.ToDictionary()));
        }

        var item = await cartService.UpsertItemAsync(User.GetUserId(), dto, ct);

        if (request.Quantity == 0)
        {
            return NoContent();
        }

        if (item is null)
        {
            return Problem(statusCode: StatusCodes.Status404NotFound, title: localizer["Error.ProductNotFound"]);
        }

        return Ok(mapper.Map<CartItemDto>(item));
    }

    [HttpDelete("items/{productId:guid}")]
    public async Task<IActionResult> RemoveItem(Guid productId, CancellationToken ct) =>
        await cartService.RemoveItemAsync(User.GetUserId(), productId, ct) ? NoContent() : NotFound();

    [HttpDelete]
    public async Task<IActionResult> ClearCart(CancellationToken ct)
    {
        await cartService.ClearAsync(User.GetUserId(), ct);
        return NoContent();
    }
}
