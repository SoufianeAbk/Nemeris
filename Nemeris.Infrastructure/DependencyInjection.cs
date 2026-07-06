using FluentValidation;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Nemeris.Core.Interfaces;
using Nemeris.Core.Mapping;
using Nemeris.Infrastructure.Data;
using Nemeris.Infrastructure.Identity;
using Nemeris.Infrastructure.Options;
using Nemeris.Infrastructure.Services;

namespace Nemeris.Infrastructure;

/// <summary>
/// Single composition root for everything Infrastructure owns. Api and Web call
/// AddInfrastructure(...) and then add only their own authentication scheme on top
/// (JWT bearer vs. Identity cookies) — that part is host-specific by design.
/// </summary>
public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddDbContext<NemerisDbContext>(options =>
            options.UseSqlServer(
                configuration.GetConnectionString("DefaultConnection"),
                sql => sql.EnableRetryOnFailure()));

        // AddIdentityCore instead of AddIdentity: registers managers and stores but
        // no cookie/auth scheme, so each host stays free to pick JWT or cookies.
        services.AddIdentityCore<ApplicationUser>(options =>
            {
                options.User.RequireUniqueEmail = true;
                options.Password.RequiredLength = 8;
                options.Password.RequireNonAlphanumeric = false;
                options.Lockout.MaxFailedAccessAttempts = 5;
                options.Lockout.DefaultLockoutTimeSpan = TimeSpan.FromMinutes(15);
            })
            .AddRoles<ApplicationRole>()
            .AddEntityFrameworkStores<NemerisDbContext>()
            .AddSignInManager()
            .AddDefaultTokenProviders();

        services.Configure<JwtOptions>(configuration.GetSection(JwtOptions.SectionName));
        services.Configure<SmtpOptions>(configuration.GetSection(SmtpOptions.SectionName));

        services.AddAutoMapper(cfg => cfg.AddMaps(typeof(MappingProfile).Assembly));
        services.AddValidatorsFromAssembly(typeof(MappingProfile).Assembly);

        services.AddScoped<IProductService, ProductService>();
        services.AddScoped<ICategoryService, CategoryService>();
        services.AddScoped<ICartService, CartService>();
        services.AddScoped<IOrderService, OrderService>();
        services.AddScoped<IReviewService, ReviewService>();
        services.AddScoped<ITokenService, TokenService>();
        services.AddScoped<IEmailService, EmailService>();

        return services;
    }
}
