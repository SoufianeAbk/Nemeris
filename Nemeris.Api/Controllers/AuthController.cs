using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Localization;
using Microsoft.Extensions.Options;
using Nemeris.Api.Extensions;
using Nemeris.Core.Entities;
using Nemeris.Core.Interfaces;
using Nemeris.Infrastructure.Data;
using Nemeris.Infrastructure.Identity;
using Nemeris.Infrastructure.Options;
using Nemeris.Shared.Auth;

namespace Nemeris.Api.Controllers;

[ApiController]
[Route("api/auth")]
public class AuthController(
    UserManager<ApplicationUser> userManager,
    SignInManager<ApplicationUser> signInManager,
    NemerisDbContext db,
    ITokenService tokenService,
    IOptions<JwtOptions> jwtOptions,
    IStringLocalizer<SharedResources> localizer) : ControllerBase
{
    [HttpPost("register")]
    public async Task<ActionResult<AuthResponse>> Register(RegisterRequest request, CancellationToken ct)
    {
        if (await userManager.FindByEmailAsync(request.Email) is not null)
        {
            return Problem(statusCode: StatusCodes.Status409Conflict, title: localizer["Auth.EmailTaken"]);
        }

        var user = new ApplicationUser
        {
            UserName = request.Email,
            Email = request.Email,
            FirstName = request.FirstName.Trim(),
            LastName = request.LastName.Trim(),
            CreatedAt = DateTime.UtcNow,
        };

        var result = await userManager.CreateAsync(user, request.Password);
        if (!result.Succeeded)
        {
            // Identity's own error descriptions (password too short, …) are user-facing already.
            return ValidationProblem(new ValidationProblemDetails(
                result.Errors
                    .GroupBy(e => e.Code)
                    .ToDictionary(g => g.Key, g => g.Select(e => e.Description).ToArray())));
        }

        await userManager.AddToRoleAsync(user, DbSeeder.CustomerRole);

        return Ok(await IssueTokensAsync(user, ct));
    }

    [HttpPost("login")]
    public async Task<ActionResult<AuthResponse>> Login(LoginRequest request, CancellationToken ct)
    {
        var user = await userManager.FindByEmailAsync(request.Email);
        if (user is null)
        {
            return Problem(statusCode: StatusCodes.Status401Unauthorized, title: localizer["Auth.InvalidCredentials"]);
        }

        // lockoutOnFailure: repeated wrong passwords trip Identity's lockout policy.
        var check = await signInManager.CheckPasswordSignInAsync(user, request.Password, lockoutOnFailure: true);
        if (check.IsLockedOut)
        {
            return Problem(statusCode: StatusCodes.Status401Unauthorized, title: localizer["Auth.AccountLocked"]);
        }

        if (!check.Succeeded)
        {
            return Problem(statusCode: StatusCodes.Status401Unauthorized, title: localizer["Auth.InvalidCredentials"]);
        }

        return Ok(await IssueTokensAsync(user, ct));
    }

    [HttpPost("refresh")]
    public async Task<ActionResult<AuthResponse>> Refresh(RefreshTokenRequest request, CancellationToken ct)
    {
        var stored = await db.RefreshTokens.FirstOrDefaultAsync(t => t.Token == request.RefreshToken, ct);
        if (stored is null)
        {
            return Problem(statusCode: StatusCodes.Status401Unauthorized, title: localizer["Auth.InvalidRefreshToken"]);
        }

        if (!stored.IsActive)
        {
            // A revoked token being replayed is a theft signal: revoke the user's
            // whole session family so the attacker's copy dies too.
            if (stored.RevokedAtUtc is not null)
            {
                var activeTokens = await db.RefreshTokens
                    .Where(t => t.UserId == stored.UserId && t.RevokedAtUtc == null)
                    .ToListAsync(ct);
                foreach (var token in activeTokens)
                {
                    token.RevokedAtUtc = DateTime.UtcNow;
                }

                await db.SaveChangesAsync(ct);
            }

            return Problem(statusCode: StatusCodes.Status401Unauthorized, title: localizer["Auth.InvalidRefreshToken"]);
        }

        var user = await userManager.FindByIdAsync(stored.UserId.ToString());
        if (user is null)
        {
            return Problem(statusCode: StatusCodes.Status401Unauthorized, title: localizer["Auth.InvalidRefreshToken"]);
        }

        // Rotation: issue a fresh pair, then chain the old token to its successor.
        var response = await IssueTokensAsync(user, ct);

        stored.RevokedAtUtc = DateTime.UtcNow;
        stored.ReplacedByToken = response.RefreshToken;
        await db.SaveChangesAsync(ct);

        return Ok(response);
    }

    [Authorize]
    [HttpPost("revoke")]
    public async Task<IActionResult> Revoke(RefreshTokenRequest request, CancellationToken ct)
    {
        var userId = User.GetUserId();

        // Scoped to the caller: nobody can revoke another user's session.
        var stored = await db.RefreshTokens
            .FirstOrDefaultAsync(t => t.Token == request.RefreshToken && t.UserId == userId, ct);
        if (stored is null)
        {
            return NotFound();
        }

        if (stored.RevokedAtUtc is null)
        {
            stored.RevokedAtUtc = DateTime.UtcNow;
            await db.SaveChangesAsync(ct);
        }

        return NoContent();
    }

    private async Task<AuthResponse> IssueTokensAsync(ApplicationUser user, CancellationToken ct)
    {
        var roles = await userManager.GetRolesAsync(user);
        var accessToken = tokenService.CreateAccessToken(user.Id, user.Email!, roles);
        var refreshToken = tokenService.GenerateRefreshToken();

        db.RefreshTokens.Add(new RefreshToken
        {
            UserId = user.Id,
            Token = refreshToken,
            ExpiresAtUtc = DateTime.UtcNow.AddDays(jwtOptions.Value.RefreshTokenDays),
        });
        await db.SaveChangesAsync(ct);

        return new AuthResponse
        {
            AccessToken = accessToken.Token,
            AccessTokenExpiresAtUtc = accessToken.ExpiresAtUtc,
            RefreshToken = refreshToken,
            UserId = user.Id,
            Email = user.Email!,
            FirstName = user.FirstName,
            LastName = user.LastName,
            Roles = [.. roles],
        };
    }
}
