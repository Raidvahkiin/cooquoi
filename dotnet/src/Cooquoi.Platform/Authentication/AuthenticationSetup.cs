using Cooquoi.Platform.Authorization;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Raidvahkiin.Ddd;
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
                jwt.Events = new JwtBearerEvents
                {
                    OnChallenge = WriteUnauthorizedProblemAsync,
                    OnForbidden = WriteForbiddenProblemAsync,
                };
            });
        builder.Services.AddAuthorizationBuilder()
            .AddPolicy(CooquoiPolicies.Curator, policy => policy
                .RequireAuthenticatedUser()
                .RequireAssertion(context => context.User.HasRealmRole(RealmRoles.CatalogCurator)));
        builder.Services.AddHttpContextAccessor();
        builder.Services.AddScoped<CurrentUser>();
        return builder;
    }

    /// <summary>401 in the contract's shape (<c>common.unauthorized</c>) instead of an empty body.</summary>
    private static async Task WriteUnauthorizedProblemAsync(JwtBearerChallengeContext context)
    {
        context.HandleResponse();
        context.Response.Headers.WWWAuthenticate = JwtBearerDefaults.AuthenticationScheme;
        await WriteProblemAsync(context.HttpContext, new Problem(StatusCodes.Status401Unauthorized, UnauthorizedCode));
    }

    /// <summary>403 in the contract's shape (<c>common.forbidden</c>), e.g. a missing curator role.</summary>
    private static Task WriteForbiddenProblemAsync(ForbiddenContext context) =>
        WriteProblemAsync(
            context.HttpContext,
            new Problem(StatusCodes.Status403Forbidden, CommonError.Forbidden.Code, CommonError.Forbidden.MessageTemplate));

    private static async Task WriteProblemAsync(HttpContext httpContext, Problem problem)
    {
        var problemDetails = httpContext.RequestServices.GetRequiredService<IProblemDetailsService>();
        await problemDetails.TryWriteAsync(httpContext, problem);
    }
}
