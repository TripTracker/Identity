﻿using IdentityServer4.Extensions;
using IdentityServer4.Models;
using IdentityServer4.Services;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;
using RestSharp;
using RestSharp.Authenticators;
using System;
using System.Collections.Generic;
using System.Security.Claims;
using System.Threading.Tasks;
using IdentityServer.Context;
using Microsoft.Extensions.Configuration;

namespace IdentityServer.Profiles
{
    public class GoogleProfileService : IProfileService
    {
        private readonly ILogger<GoogleProfileService> Logger;
        private readonly IConfiguration Configuration;
        private readonly IRestClient Client;
        private readonly UserManager<TripTreckerUser> UserManager;

        public GoogleProfileService(ILogger<GoogleProfileService> logger, IRestClient client, UserManager<TripTreckerUser> userManager, IConfiguration configuration)
        {
            Logger = logger;
            Client = client;
            UserManager = userManager;
            Configuration = configuration;
        }

        public async Task GetProfileDataAsync(ProfileDataRequestContext context)
        {
            var googleToken = context.ValidatedRequest?.Raw["token"];

            if (googleToken != null)
            {
                await LookupSetAndReturnUserGoogleClaims(context, googleToken);
                return;
            }
            else
            {
                await CheckForAndReturnExistingGoogleClaims(context);
            }
        }

        public async Task IsActiveAsync(IsActiveContext context)
        {
            // WE DO NOT USE THIS AS WE RELY ON GOOGLE AUTH. REQUIRED FOR INTERFACE
        }

        private async Task LookupSetAndReturnUserGoogleClaims(ProfileDataRequestContext context, string googleToken)
        {
            var userInfo = await GetGoogleUserInfo(googleToken);

            context.IssuedClaims.AddRange(new List<Claim>()
            {
                new Claim("email", userInfo.Email),
                new Claim("name", userInfo.Name),
                new Claim("picture", userInfo.Picture)
            });

            var sub = context.Subject.GetSubjectId();

            var user = await UserManager.FindByIdAsync(sub);
            user.Picture = userInfo.Picture;
            user.Name = userInfo.Name;

            await UserManager.UpdateAsync(user);
        }

        private async Task CheckForAndReturnExistingGoogleClaims(ProfileDataRequestContext context)
        {
            var sub = context.Subject.GetSubjectId();
            var user = await UserManager.FindByIdAsync(sub);

            if (user != null)
            {
                context.IssuedClaims.AddRange(new List<Claim>()
                {
                    new Claim("email", user.Email),
                    new Claim("name", user.Name),
                    new Claim("picture", user.Picture)
                });
            }
        }

        // to do => clean this up during cleanup phase
        private async Task<UserProfile> GetGoogleUserInfo(string token)
        {
            try
            {
                Client.Authenticator = new JwtAuthenticator(token);

                var request = new RestRequest(Configuration["GOOGLE_USERINFOURL"], DataFormat.Json);
                var result = await Client.ExecuteAsync<UserProfile>(request);

                return result.Data;
            }
            catch (Exception ex)
            {
                Logger.LogError("Request to get google user info failed", ex);
                throw ex;
            }
        }
    }
}
