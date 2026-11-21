using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Raidvahkiin.Ddd.AspNetCore;

namespace Cooquoi.Platform.Authentication;

public static class AuthenticationSetup
{
    public const string UnauthorizedCode = "common.unauthorized";

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
                jwt.Events = new JwtBearerEvents { OnChallenge = WriteUnauthorizedProblemAsync };
            });
        builder.Services.AddAuthorization();
        builder.Services.AddHttpContextAccessor();
        builder.Services.AddScoped<CurrentUser>();
        return builder;
    }

    /// <summary>401 in the contract's shape (<c>common.unauthorized</c>) instead of an empty body.</summary>
    private static async Task WriteUnauthorizedProblemAsync(JwtBearerChallengeContext context)
    {
        context.HandleResponse();
        context.Response.Headers.WWWAuthenticate = JwtBearerDefaults.AuthenticationScheme;
        var problemDetails = context.HttpContext.RequestServices.GetRequiredService<IProblemDetailsService>();
        await problemDetails.TryWriteAsync(context.HttpContext, new Problem(StatusCodes.Status401Unauthorized, UnauthorizedCode));
    }
}
