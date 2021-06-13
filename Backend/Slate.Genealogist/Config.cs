// Copyright (c) Brock Allen & Dominick Baier. All rights reserved.
// Licensed under the Apache License, Version 2.0. See LICENSE in the project root for license information.


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
                    SubjectId = "1",
                    Username = "atomicblom",
                    Password = "password",
                    Claims = new[]
                    {
                        new Claim("username", "AtomicBlom")
                    }
                },
                new TestUser
                {
                    SubjectId = "2",
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