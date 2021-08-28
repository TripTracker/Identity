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

namespace IdentityServer.Profiles
{
    public class GoogleProfileService : IProfileService
    {
        private readonly ILogger<GoogleProfileService> _logger;
        private readonly IRestClient _client;
        private readonly UserManager<TripTreckerUser> _userManager;

        public GoogleProfileService(ILogger<GoogleProfileService> logger, IRestClient client, UserManager<TripTreckerUser> userManager)
        {
            _logger = logger;
            _client = client;
            _userManager = userManager;
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

            context.AddRequestedClaims(new List<Claim>()
            {
                new Claim("email", userInfo.Email),
                new Claim("name", userInfo.Name),
                new Claim("picture", userInfo.Picture)
            });

            var sub = context.Subject.GetSubjectId();

            var user = await _userManager.FindByIdAsync(sub);
            user.Picture = userInfo.Picture;
            user.Name = userInfo.Name;

            await _userManager.UpdateAsync(user);
        }

        private async Task CheckForAndReturnExistingGoogleClaims(ProfileDataRequestContext context)
        {
            var sub = context.Subject.GetSubjectId();
            var user = await _userManager.FindByIdAsync(sub);

            if (user != null)
            {
                context.AddRequestedClaims(new List<Claim>()
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
                _client.Authenticator = new JwtAuthenticator(token);

                var request = new RestRequest("https://www.googleapis.com/oauth2/v1/userinfo", DataFormat.Json);
                var result = await _client.ExecuteAsync<UserProfile>(request);

                return result.Data;
            }
            catch (Exception ex)
            {
                // TO DO LOGGING AND RETURN RESULT
                throw ex;
            }
        }
    }
}
