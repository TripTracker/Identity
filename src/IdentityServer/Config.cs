// Copyright (c) Brock Allen & Dominick Baier. All rights reserved.
// Licensed under the Apache License, Version 2.0. See LICENSE in the project root for license information.


using IdentityServer4;
using IdentityServer4.Models;
using System.Collections.Generic;

namespace IdentityServer
{
    public static class Config
    {
        public static IEnumerable<IdentityResource> IdentityResources =>
            new IdentityResource[]
            { 
                new IdentityResources.OpenId(),

                new IdentityResource(
                    name: "profile",
                    userClaims: new[] { "sub", "email", "picture", "name" },
                    displayName: "Profile claims"),
            };

        public static IEnumerable<ApiScope> ApiScopes =>
            new ApiScope[]
            {
                new ApiScope(ClientScopes.TRIP_API, "TripApi"),
                new ApiScope(ClientScopes.LOCATION_API, "LocationApi"),
                new ApiScope(ClientScopes.GRAPH, "Graph"),
                new ApiScope(ClientScopes.CONTENT_API, "ContentApi"),
            };

        public static IEnumerable<Client> Clients =>
            new Client[] 
            {
                new Client
                {
                    ClientId = ServerClients.GRAPH,

                    // no interactive user, use the clientid/secret for authentication
                    AllowedGrantTypes = GrantTypes.Code,

                    // secret for authentication
                    ClientSecrets =
                    {
                        new Secret("fake_secret".Sha256())
                    },

                    // scopes that client has access to
                    AllowedScopes = 
                    { 
                         ClientScopes.TRIP_API,
                         ClientScopes.LOCATION_API,
                         ClientScopes.CONTENT_API
                    }
                },
                new Client
                {
                    ClientId = ServerClients.CONTENTAPI,

                    // no interactive user, use the clientid/secret for authentication
                    AllowedGrantTypes = GrantTypes.Code,

                    // secret for authentication
                    ClientSecrets =
                    {
                        new Secret("fake_secret".Sha256())
                    },

                    // scopes that client has access to
                    AllowedScopes =
                    {
                         ClientScopes.TRIP_API
                    }
                },
                new Client
                {
                    ClientId = ServerClients.TRIPAPI,

                    // no interactive user, use the clientid/secret for authentication
                    AllowedGrantTypes = GrantTypes.Code,

                    // secret for authentication
                    ClientSecrets =
                    {
                        new Secret("fake_secret".Sha256())
                    },

                    // scopes that client has access to
                    AllowedScopes =
                    {
                         ClientScopes.LOCATION_API
                    }
                },
                new Client
                {
                    ClientId = ServerClients.VUE,
                    AllowedGrantTypes = new[] { "delegation" },
                    RequireClientSecret = false,

                    RedirectUris = { "https://localhost:8080/callback" },
                    PostLogoutRedirectUris = { "https://localhost:8080/index.html" },
                    AllowedCorsOrigins = { "https://localhost:8080" },
                    AlwaysIncludeUserClaimsInIdToken = true,
                    AlwaysSendClientClaims = true,
                    AccessTokenType = AccessTokenType.Jwt,

                    // scopes that client has access to
                    AllowedScopes =
                    {
                         IdentityServerConstants.StandardScopes.OpenId,
                         IdentityServerConstants.StandardScopes.Profile,
                         IdentityServerConstants.StandardScopes.Email,
                         ClientScopes.TRIP_API,
                         ClientScopes.LOCATION_API,
                         ClientScopes.CONTENT_API,
                         ClientScopes.GRAPH
                    },
                },
            };
    }
}