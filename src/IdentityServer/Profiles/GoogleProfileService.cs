using IdentityServer4.Extensions;
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
                new Claim("firstName", userInfo.FirstName),
                new Claim("lastName", userInfo.LastName),
                new Claim("picture", userInfo.Picture),
                new Claim(ClaimTypes.Name, userInfo.Email)
            });

            await UpdateUserIdentityWithGoogleInfo(userInfo, context);
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
                    new Claim("firstName", user.FirstName),
                    new Claim("lastName", user.LastName),
                    new Claim("picture", user.Picture),
                    new Claim(ClaimTypes.Name, user.Email)
                });
            }
        }

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

        private async Task UpdateUserIdentityWithGoogleInfo(UserProfile userInfo, ProfileDataRequestContext context)
        {
            var sub = context.Subject.GetSubjectId();

            var user = await UserManager.FindByIdAsync(sub);
            user.Picture = userInfo.Picture;
            user.FirstName = userInfo.FirstName;
            user.LastName = userInfo.LastName;

            await UserManager.UpdateAsync(user);
        }
    }
}
