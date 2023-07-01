using ApplicationCore.Entities.User;
using ApplicationCore.Interfaces;
using ApplicationCore.Interfaces.Base;
using Infrastructure.Data;
using Infrastructure.Repositories;
using Infrastructure.Services;
using Infrastructure.User;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace PublicApi.Config;

public static class ConfigService
{
    public static void Configure(IConfiguration configuration, IServiceCollection services)
    {
        var useOnlyInMemoryDatabase = false;
        if (configuration["UseOnlyInMemoryDatabase"] != null)
            useOnlyInMemoryDatabase = bool.Parse(configuration["UseOnlyInMemoryDatabase"]!);

        if (useOnlyInMemoryDatabase)
        {
            services.AddDbContext<AppIdentityDbContext>(
                options => options.UseInMemoryDatabase("Identity")
            );
        }
        else
        {
            services.AddDbContext<AppDbContext>(
                c => c.UseNpgsql(configuration.GetConnectionString("IdentityConnection"))
            );
            services.AddDbContext<AppIdentityDbContext>(
                options =>
                    options.UseNpgsql(configuration.GetConnectionString("IdentityConnection"))
            );
        }

        services
            .AddIdentity<ApplicationUser, IdentityRole>()
            .AddEntityFrameworkStores<AppIdentityDbContext>()
            .AddDefaultTokenProviders();

        AddScope(services);
        services.AddScoped<ITokenClaimsService, IdentityTokenClaimService>();
        services.AddAutoMapper(AppDomain.CurrentDomain.GetAssemblies());
    }

    private static void AddScope(IServiceCollection services)
    {
        services.AddScoped(typeof(IRepository<>), typeof(EfRepository<>));
    }
}
