using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace Cooquoi.Platform.Authentication;

public static class AuthenticationSetup
{
    /// <summary>Bearer-only API (ADR 0011): validates Keycloak access tokens.</summary>
    public static IHostApplicationBuilder AddCooquoiAuthentication(this IHostApplicationBuilder builder)
    {
        builder.Services.AddOptions<CooquoiAuthenticationOptions>()
            .BindConfiguration(CooquoiAuthenticationOptions.Section)
            .ValidateDataAnnotations()
            .ValidateOnStart();

        var options = builder.Configuration
            .GetSection(CooquoiAuthenticationOptions.Section)
            .Get<CooquoiAuthenticationOptions>() ?? new CooquoiAuthenticationOptions();

        builder.Services
            .AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
            .AddJwtBearer(jwt =>
            {
                jwt.Authority = options.Authority;
                jwt.Audience = options.Audience;
                jwt.MapInboundClaims = false;
                jwt.TokenValidationParameters.NameClaimType = "name";
            });
        builder.Services.AddAuthorization();
        builder.Services.AddHttpContextAccessor();
        builder.Services.AddScoped<ICurrentUser, CurrentUser>();
        return builder;
    }
}
