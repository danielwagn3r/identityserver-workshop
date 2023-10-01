using System.Net;
using Duende.IdentityServer;
using IdentityModel;
using Microsoft.IdentityModel.Tokens;
using Serilog;

namespace IdentityServer;

internal static class HostingExtensions
{
    public static WebApplication ConfigureServices(this WebApplicationBuilder builder)
    {
        // uncomment if you want to add a UI
        builder.Services.AddRazorPages();

        builder.Services.AddIdentityServer(options =>
            {
                // https://docs.duendesoftware.com/identityserver/v6/fundamentals/resources/api_scopes#authorization-based-on-scopes
                options.EmitStaticAudienceClaim = true;

                options.Events.RaiseSuccessEvents = true;
                options.Events.RaiseErrorEvents = true;
                options.Events.RaiseFailureEvents = true;
                options.Events.RaiseInformationEvents = true;
            })
            .AddInMemoryIdentityResources(Config.IdentityResources)
            .AddInMemoryApiScopes(Config.ApiScopes)
            .AddInMemoryApiResources(Config.ApiResources)
            .AddInMemoryClients(Config.Clients)
            .AddServerSideSessions()
            .AddTestUsers(TestUsers.Users)
            .AddAppAuthRedirectUriValidator();

        // add external AAD authentication
        builder.Services.AddAuthentication()
            .AddOpenIdConnect("AAD", "Azure AD", options =>
            {
                options.Authority = "https://login.microsoftonline.com/f17beaad-aa93-459d-80ff-ac449746eeb5/v2.0";
                options.ClientId = "dec49b01-c768-4080-b824-1529ad55d329";
                options.ClientSecret = "Gxu8Q~wgiVP-MX0tSMJOnoFICa2~M5NcKlm4ldfV";

                options.SignInScheme = IdentityServerConstants.ExternalCookieAuthenticationScheme;

                options.ResponseType = OidcConstants.ResponseTypes.Code;
                options.UsePkce = true;

                options.SaveTokens = true;
                options.GetClaimsFromUserInfoEndpoint = true;

                options.CallbackPath = "/signin-aad";
                options.RemoteSignOutPath = "/signout-aad";

                options.SignedOutCallbackPath = "/signedout-add";

                options.BackchannelHttpHandler = new HttpClientHandler
                {
                    DefaultProxyCredentials = CredentialCache.DefaultCredentials
                };

                options.TokenValidationParameters = new TokenValidationParameters
                {
                    NameClaimType = "name",
                    RoleClaimType = "role"
                };
            });

        return builder.Build();
    }
    
    public static WebApplication ConfigurePipeline(this WebApplication app)
    { 
        app.UseSerilogRequestLogging();
    
        if (app.Environment.IsDevelopment())
        {
            app.UseDeveloperExceptionPage();
        }

        // uncomment if you want to add a UI
        app.UseStaticFiles();
        app.UseRouting();
            
        app.UseIdentityServer();

        // uncomment if you want to add a UI
        app.UseAuthorization();
        app.MapRazorPages().RequireAuthorization();

        return app;
    }
}
