using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Localization;
using Nemeris.Core.Dtos;
using Nemeris.Core.Interfaces;
using Nemeris.Infrastructure.Data;
using Nemeris.Infrastructure.Identity;
using Nemeris.Web.Extensions;
using Nemeris.Web.Models;

namespace Nemeris.Web.Controllers;

public class AccountController(
    UserManager<ApplicationUser> userManager,
    SignInManager<ApplicationUser> signInManager,
    IOrderService orderService,
    IAddressService addressService,
    IStringLocalizer<SharedResources> localizer) : Controller
{
    [HttpGet]
    public IActionResult Login(string? returnUrl = null) =>
        View(new LoginViewModel { ReturnUrl = returnUrl });

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Login(LoginViewModel model)
    {
        if (!ModelState.IsValid)
        {
            return View(model);
        }

        // UserName == Email at registration, so email works as the sign-in name.
        var result = await signInManager.PasswordSignInAsync(
            model.Email, model.Password, model.RememberMe, lockoutOnFailure: true);

        if (result.IsLockedOut)
        {
            ModelState.AddModelError(string.Empty, localizer["Auth.AccountLocked"]);
            return View(model);
        }

        if (!result.Succeeded)
        {
            ModelState.AddModelError(string.Empty, localizer["Auth.InvalidCredentials"]);
            return View(model);
        }

        return LocalRedirect(Url.IsLocalUrl(model.ReturnUrl) ? model.ReturnUrl! : "/");
    }

    [HttpGet]
    public IActionResult Register() => View(new RegisterViewModel());

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Register(RegisterViewModel model)
    {
        if (!ModelState.IsValid)
        {
            return View(model);
        }

        var user = new ApplicationUser
        {
            UserName = model.Email,
            Email = model.Email,
            FirstName = model.FirstName.Trim(),
            LastName = model.LastName.Trim(),
            CreatedAt = DateTime.UtcNow,
        };

        var result = await userManager.CreateAsync(user, model.Password);
        if (!result.Succeeded)
        {
            foreach (var error in result.Errors)
            {
                ModelState.AddModelError(string.Empty, error.Description);
            }

            return View(model);
        }

        await userManager.AddToRoleAsync(user, DbSeeder.CustomerRole);
        await signInManager.SignInAsync(user, isPersistent: false);

        return RedirectToAction("Index", "Home");
    }

    [Authorize]
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Logout()
    {
        await signInManager.SignOutAsync();
        return RedirectToAction("Index", "Home");
    }

    [Authorize]
    [HttpGet]
    public async Task<IActionResult> Orders(int page = 1, CancellationToken ct = default) =>
        View(await orderService.GetForUserAsync(User.GetUserId(), page, 10, ct));

    [Authorize]
    [HttpGet]
    public async Task<IActionResult> Addresses(CancellationToken ct) =>
        View(await addressService.GetForUserAsync(User.GetUserId(), ct));

    [Authorize]
    [HttpGet]
    public IActionResult AddAddress(string? returnUrl = null) =>
        View(new AddressViewModel { ReturnUrl = returnUrl });

    [Authorize]
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> AddAddress(AddressViewModel model, CancellationToken ct)
    {
        if (!ModelState.IsValid)
        {
            return View(model);
        }

        await addressService.CreateAsync(User.GetUserId(), new AddressUpsertDto
        {
            FullName = model.FullName,
            Street = model.Street,
            City = model.City,
            PostalCode = model.PostalCode,
            Country = model.Country.ToUpperInvariant(),
            PhoneNumber = model.PhoneNumber,
            IsDefault = model.IsDefault,
        }, ct);

        return LocalRedirect(Url.IsLocalUrl(model.ReturnUrl) ? model.ReturnUrl! : Url.Action(nameof(Addresses))!);
    }

    [HttpGet]
    public IActionResult AccessDenied()
    {
        TempData["Error"] = localizer["Auth.AccessDenied"].Value;
        return RedirectToAction("Index", "Home");
    }
}
