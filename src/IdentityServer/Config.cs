using Duende.IdentityServer.Models;

namespace IdentityServer;

public static class Config
{
    public static IEnumerable<IdentityResource> IdentityResources =>
        new IdentityResource[]
        {
            new IdentityResources.OpenId(),
            new IdentityResources.Profile(),
            new IdentityResources.Email()
        };

    public static IEnumerable<ApiScope> ApiScopes =>
        new ApiScope[]
        {
            new ApiScope("calc:double", "Double a number"),
            new ApiScope("calc:square", "Square a number"),
        };

    public static IEnumerable<ApiResource> ApiResources =>
        new ApiResource[]
        {
            new ApiResource("urn:calcapi", "Calculator API")
            {
                Scopes = { "calc:double", "calc:square" },

                RequireResourceIndicator = true
            }
        };

    public static IEnumerable<Client> Clients =>
        new Client[]
        {
            new Client
            {
                ClientId = "console",
                ClientName = "Console Client",
                // no interactive user, use the clientid/secret for authentication
                AllowedGrantTypes = GrantTypes.ClientCredentials,
                // secret for authentication
                ClientSecrets =
                {
                    new Secret("secret".Sha256())
                },
                // scopes that client has access to
                AllowedScopes = {
                    "calc:double",
                    "calc:square"
                },
            },
            new Client
            {
                ClientId = "worker",
                ClientName = "RefreshClient",
                // no interactive user, use the clientid/secret for authentication
                AllowedGrantTypes = GrantTypes.ClientCredentials,
                // secret for authentication
                ClientSecrets =
                {
                    new Secret("secret".Sha256())
                },
                // scopes that client has access to
                AllowedScopes = {
                    "calc:double",
                    "calc:square"
                },

                AccessTokenLifetime = 75
            },
            new Client
            {
                ClientId = "web",
                ClientSecrets = { new Secret("secret".Sha256()) },
                ClientName = "Web Client",

                AllowedGrantTypes = GrantTypes.Code,
                RequirePkce = true,

                RedirectUris = { "https://localhost:5002/signin-oidc" },
                FrontChannelLogoutUri = "https://localhost:5002/signout-oidc",
                PostLogoutRedirectUris = { "https://localhost:5002/signout-callback-oidc" },

                AllowOfflineAccess = true,
                AllowedScopes = { "openid", "profile", "email", "calc:double", "calc:square" },

                RequireConsent = true
            }
        };
}