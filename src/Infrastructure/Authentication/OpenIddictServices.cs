using System.Globalization;
using System.Reflection;
using System.Text.Json;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using OpenIddict.Abstractions;
using OpenIddict.Server;
using Quartz;
using ServerGame.Infrastructure.Data;

namespace ServerGame.Infrastructure.Authentication;

public static class OpenIddictServices
{
    public static IHostApplicationBuilder AddOpenIddictBuilder(this IHostApplicationBuilder builder)
    {
        // já carrega appsettings.json e appsettings.{ENV}.json…
        builder.Configuration
            .AddJsonFile("appsettings.OpenIddict.json",
                optional: false,
                reloadOnChange: true);
        
        var configuration = builder.Configuration;
        
        // OpenIddict offers native integration with Quartz.NET to perform scheduled tasks
        // (like pruning orphaned authorizations/tokens from the database) at regular intervals.
        builder.Services.AddQuartz(options =>
        {
            options.UseSimpleTypeLoader();
            options.UseInMemoryStore();
        });
        
        // Register the Quartz.NET service and configure it to block shutdown until jobs are complete.
        builder.Services.AddQuartzHostedService(options => options.WaitForJobsToComplete = true);
        
        builder.Services.AddOpenIddict()
            .AddCore(options =>
            {
                options.UseEntityFrameworkCore()
                    .UseDbContext<ApplicationDbContext>();
                
                // Enable Quartz.NET integration.
                options.UseQuartz();
            })
            .AddServer(options =>
            {
                // Enable the authorization, token, introspection and userinfo endpoints.
                options.SetAuthorizationEndpointUris(configuration["OpenIddict:Endpoints:Authorization"]!)
                    .SetTokenEndpointUris(configuration["OpenIddict:Endpoints:Token"]!)
                    .SetIntrospectionEndpointUris(configuration["OpenIddict:Endpoints:Introspection"]!)
                    .SetUserInfoEndpointUris(configuration["OpenIddict:Endpoints:Userinfo"]!)
                    .SetEndSessionEndpointUris(configuration["OpenIddict:Endpoints:Logout"]!);
                
                // Enable the authorization code, implicit, hybrid and the refresh token flows.
                options.AllowAuthorizationCodeFlow()
                    .AllowImplicitFlow()
                    .AllowHybridFlow()
                    .AllowRefreshTokenFlow();

                // Expose all the supported claims in the discovery document.
                options.RegisterClaims(configuration.GetSection("OpenIddict:Claims").Get<string[]>()!);

                // Expose all the supported scopes in the discovery document.
                options.RegisterScopes(configuration.GetSection("OpenIddict:Scopes").Get<string[]>()!);

                // Note: an ephemeral signing key is deliberately used to make the "OP-Rotation-OP-Sig"
                // test easier to run as restarting the application is enough to rotate the keys.
                options.AddEphemeralEncryptionKey()
                    .AddEphemeralSigningKey();

                // Register the ASP.NET Core host and configure the ASP.NET Core-specific options.
                //
                // Note: the pass-through mode is not enabled for the token endpoint
                // so that token requests are automatically handled by OpenIddict.
                options.UseAspNetCore()
                    .EnableAuthorizationEndpointPassthrough()
                    .EnableEndSessionEndpointPassthrough();
                
                // Register the custom event handler responsible for populating userinfo responses.
                options.AddEventHandler<OpenIddictServerEvents.HandleUserInfoRequestContext>(options => options.UseInlineHandler(context =>
                {
                    if (context.Principal.HasScope(OpenIddictConstants.Permissions.Scopes.Profile))
                    {
                        context.GivenName = context.Principal.GetClaim(OpenIddictConstants.Claims.GivenName);
                        context.FamilyName = context.Principal.GetClaim(OpenIddictConstants.Claims.FamilyName);
                        context.BirthDate = context.Principal.GetClaim(OpenIddictConstants.Claims.Birthdate);
                        context.Profile = context.Principal.GetClaim(OpenIddictConstants.Claims.Profile);
                        context.PreferredUsername = context.Principal.GetClaim(OpenIddictConstants.Claims.PreferredUsername);
                        context.Website = context.Principal.GetClaim(OpenIddictConstants.Claims.Website);

                        context.Claims[OpenIddictConstants.Claims.Name] = context.Principal.GetClaim(OpenIddictConstants.Claims.Name);
                        context.Claims[OpenIddictConstants.Claims.Gender] = context.Principal.GetClaim(OpenIddictConstants.Claims.Gender);
                        context.Claims[OpenIddictConstants.Claims.MiddleName] = context.Principal.GetClaim(OpenIddictConstants.Claims.MiddleName);
                        context.Claims[OpenIddictConstants.Claims.Nickname] = context.Principal.GetClaim(OpenIddictConstants.Claims.Nickname);
                        context.Claims[OpenIddictConstants.Claims.Picture] = context.Principal.GetClaim(OpenIddictConstants.Claims.Picture);
                        context.Claims[OpenIddictConstants.Claims.Locale] = context.Principal.GetClaim(OpenIddictConstants.Claims.Locale);
                        context.Claims[OpenIddictConstants.Claims.Zoneinfo] = context.Principal.GetClaim(OpenIddictConstants.Claims.Zoneinfo);
                        context.Claims[OpenIddictConstants.Claims.UpdatedAt] = long.Parse(
                            context.Principal.GetClaim(OpenIddictConstants.Claims.UpdatedAt)!,
                            NumberStyles.Number, CultureInfo.InvariantCulture);
                    }

                    if (context.Principal.HasScope(OpenIddictConstants.Permissions.Scopes.Email))
                    {
                        context.Email = context.Principal.GetClaim(OpenIddictConstants.Claims.Email);
                        context.EmailVerified = false;
                    }

                    if (context.Principal.HasScope(OpenIddictConstants.Permissions.Scopes.Phone))
                    {
                        context.PhoneNumber = context.Principal.GetClaim(OpenIddictConstants.Claims.PhoneNumber);
                        context.PhoneNumberVerified = false;
                    }

                    if (context.Principal.HasScope(OpenIddictConstants.Permissions.Scopes.Address))
                    {
                        context.Address = JsonSerializer.Deserialize<JsonElement>(context.Principal.GetClaim(OpenIddictConstants.Claims.Address)!);
                    }

                    return default;
                }));
            })
            .AddValidation(options =>
            {
                // Import the configuration from the local OpenIddict server instance.
                options.UseLocalServer();

                // Register the ASP.NET Core host.
                options.UseAspNetCore();

                // Enable authorization entry validation, which is required to be able
                // to reject access tokens retrieved from a revoked authorization code.
                options.EnableAuthorizationEntryValidation();
            });

        return builder;
    }
}
