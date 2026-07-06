using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Localization;
using Nemeris.Core.Dtos;
using Nemeris.Core.Interfaces;
using Nemeris.Web.Extensions;
using Nemeris.Web.Models;

namespace Nemeris.Web.Controllers;

[Authorize]
public class CartController(
    ICartService cartService,
    IOrderService orderService,
    IAddressService addressService,
    IStringLocalizer<SharedResources> localizer) : Controller
{
    public async Task<IActionResult> Index(CancellationToken ct) =>
        View(new CartViewModel { Items = await cartService.GetItemsAsync(User.GetUserId(), ct) });

    /// <summary>Storefront "add to cart": increments on top of what is already in the cart.</summary>
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Add(Guid productId, int quantity = 1, CancellationToken ct = default)
    {
        var userId = User.GetUserId();

        var existing = (await cartService.GetItemsAsync(userId, ct))
            .FirstOrDefault(i => i.ProductId == productId);
        var newQuantity = Math.Clamp((existing?.Quantity ?? 0) + Math.Max(1, quantity), 1, 999);

        var item = await cartService.UpsertItemAsync(
            userId, new CartItemUpsertDto { ProductId = productId, Quantity = newQuantity }, ct);

        TempData[item is null ? "Error" : "Success"] =
            localizer[item is null ? "Error.ProductNotFound" : "Cart.Added"].Value;

        return RedirectToAction(nameof(Index));
    }

    /// <summary>Quantity edit from the cart page: absolute set, 0 removes the line.</summary>
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Update(Guid productId, int quantity, CancellationToken ct)
    {
        await cartService.UpsertItemAsync(
            User.GetUserId(),
            new CartItemUpsertDto { ProductId = productId, Quantity = Math.Clamp(quantity, 0, 999) },
            ct);
        return RedirectToAction(nameof(Index));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Remove(Guid productId, CancellationToken ct)
    {
        await cartService.RemoveItemAsync(User.GetUserId(), productId, ct);
        return RedirectToAction(nameof(Index));
    }

    [HttpGet]
    public async Task<IActionResult> Checkout(CancellationToken ct)
    {
        var addresses = await addressService.GetForUserAsync(User.GetUserId(), ct);
        if (addresses.Count == 0)
        {
            TempData["Info"] = localizer["Checkout.NoAddress"].Value;
            return RedirectToAction("AddAddress", "Account", new { returnUrl = Url.Action(nameof(Checkout)) });
        }

        return View(new CheckoutViewModel
        {
            Addresses = addresses,
            SelectedAddressId = (addresses.FirstOrDefault(a => a.IsDefault) ?? addresses[0]).Id,
        });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Checkout(CheckoutViewModel model, CancellationToken ct)
    {
        try
        {
            var order = await orderService.CheckoutAsync(
                User.GetUserId(),
                new CheckoutDto { ShippingAddressId = model.SelectedAddressId, Notes = model.Notes },
                ct);

            return RedirectToAction(nameof(Confirmation), new { id = order.Id });
        }
        catch (InvalidOperationException ex)
        {
            // Service throws resource keys (Error.CartEmpty, …); show them localized.
            TempData["Error"] = localizer[ex.Message].Value;
            return RedirectToAction(nameof(Index));
        }
    }

    [HttpGet]
    public async Task<IActionResult> Confirmation(Guid id, CancellationToken ct)
    {
        // Scoped to the owner: someone else's order id is a 404 here.
        var order = await orderService.GetByIdAsync(id, User.GetUserId(), ct);
        return order is null ? NotFound() : View(order);
    }
}
