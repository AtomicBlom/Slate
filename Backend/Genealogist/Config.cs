// Copyright (c) Brock Allen & Dominick Baier. All rights reserved.
// Licensed under the Apache License, Version 2.0. See LICENSE in the project root for license information.


using IdentityServer4.Models;
using System.Collections.Generic;
using IdentityServer4.Test;

namespace Genealogist
{
    public static class Config
    {
        public static IEnumerable<IdentityResource> IdentityResources =>
            new IdentityResource[]
            { 
                new IdentityResources.OpenId() {UserClaims = new List<string>()
                {
                    "name"

                }}
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

                    ClientSecrets = {
                        new Secret("secret".Sha256())
                    },

                    AllowedScopes = { "account" }
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
                    Password = "password"
                },
                new TestUser
                {
                    SubjectId = "2",
                    Username = "rosethethorn",
                    Password = "password"
                }
            };
        }

    }
}