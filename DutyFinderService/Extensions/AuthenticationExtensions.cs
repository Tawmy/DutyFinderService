using AspNetCoreExtensions;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi;

namespace DutyFinderService.Extensions;

public static class AuthenticationExtensions
{
    private const string SecurityScheme = "Keycloak";

    extension(WebApplicationBuilder builder)
    {
        public void AddCustomAuthentication()
        {
            builder.Services.AddAuthentication(options =>
                {
                    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
                    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
                })
                .AddJwtBearer(options =>
                {
                    options.Authority =
                        builder.Configuration.GetGuardedConfiguration(EnvironmentVariables.AuthAuthority);
                    options.Audience =
                        builder.Configuration.GetGuardedConfiguration(EnvironmentVariables.AuthAudience);

                    options.TokenValidationParameters = new TokenValidationParameters
                    {
                        ValidateIssuerSigningKey = true,
                        ValidateIssuer = true,
                        ValidateActor = true,
                        ValidateAudience = true,
                        ValidateTokenReplay = true
                    };
                });
        }

        public void AddCustomOpenApi()
        {
            builder.Services.AddOpenApi(options =>
            {
                options.AddDocumentTransformer((document, _, _) =>
                {
                    var authority =
                        builder.Configuration.GetGuardedConfiguration(EnvironmentVariables.AuthAuthority);
                    var tokenUrl = Path.Combine(authority, "protocol/openid-connect/token");

                    string[] scopes =
                    [
                        builder.Configuration.GetGuardedConfiguration(EnvironmentVariables.AuthAudience)
                    ];
                    var scopesDict = scopes.ToDictionary(x => x, _ => "Enable me!");

                    document.Components ??= new OpenApiComponents();
                    document.Components.SecuritySchemes ??= new Dictionary<string, IOpenApiSecurityScheme>();
                    document.Components.SecuritySchemes.Add(SecurityScheme, new OpenApiSecurityScheme
                    {
                        Type = SecuritySchemeType.OAuth2,
                        Flows = new OpenApiOAuthFlows
                        {
                            ClientCredentials = new OpenApiOAuthFlow
                            {
                                TokenUrl = new Uri(tokenUrl),
                                Scopes = scopesDict
                            }
                        }
                    });

                    return Task.CompletedTask;
                });

                options.AddOperationTransformer((operation, context, _) =>
                {
                    if (context.Description.ActionDescriptor.EndpointMetadata.All(x => x is not AuthorizeAttribute))
                    {
                        return Task.CompletedTask;
                    }

                    var audience = builder.Configuration.GetGuardedConfiguration(EnvironmentVariables.AuthAudience);

                    operation.Security ??= [];
                    operation.Security.Add(new OpenApiSecurityRequirement
                    {
                        [new OpenApiSecuritySchemeReference(SecurityScheme, context.Document)] = [audience]
                    });

                    return Task.CompletedTask;
                });
            });
        }
    }
}