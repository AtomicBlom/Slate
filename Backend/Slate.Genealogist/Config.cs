using System;
using System.Collections.Generic;
using System.Security.Claims;
using IdentityServer4.Models;
using IdentityServer4.Test;

namespace Slate.Genealogist
{
    public static class Config
    {
        public static IEnumerable<IdentityResource> IdentityResources =>
            new IdentityResource[]
            { 
                new IdentityResources.OpenId() { UserClaims = {"username"}},
                new IdentityResources.Profile(),
            };

        public static IEnumerable<ApiScope> ApiScopes =>
            new[]
            {
                new ApiScope("account", "User Accounts")
            };

        public static IEnumerable<Client> Clients =>
            new[] 
            { 
                new Client {
                    ClientId = "Launcher",
                    AllowedGrantTypes = GrantTypes.ResourceOwnerPassword,
                    AllowOfflineAccess = true,
                    ClientSecrets = {
                        new Secret("secret".Sha256())
                    },
                    RefreshTokenUsage = TokenUsage.OneTimeOnly,
                    AllowedScopes = { "account", "offline_access" }
                },
                new Client
                {
                    ClientId = "GameWarden",
                    AllowedGrantTypes = GrantTypes.ClientCredentials,
                }
            };

        public static List<TestUser> GetUsers()
        {
            return new List<TestUser>
            {
                new TestUser
                {
                    SubjectId = Guid.Parse("11FB4912-2EEE-4A2C-96C9-16D1AB267184").ToString(),
                    Username = "atomicblom",
                    Password = "password",
                    Claims = new[]
                    {
                        new Claim("username", "AtomicBlom")
                    }
                },
                new TestUser
                {
                    SubjectId = Guid.Parse("050FF185B3544E08905F6D6B2782D5F1").ToString(),
                    Username = "rosethethorn",
                    Password = "password",
                    Claims = new[]
                    {
                        new Claim("username", "rosethethorn")
                    }
                }
            };
        }

    }
}