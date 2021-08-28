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

namespace IdentityServer.Profiles
{
    public class GoogleProfileService : IProfileService
    {
        private readonly ILogger<GoogleProfileService> _logger;
        private readonly IRestClient _client;
        private readonly UserManager<IdentityUser> _userManager;

        public GoogleProfileService(ILogger<GoogleProfileService> logger, IRestClient client, UserManager<IdentityUser> userManager)
        {
            _logger = logger;
            _client = client;
            _userManager = userManager;
        }

        public async Task GetProfileDataAsync(ProfileDataRequestContext context)
        {
            var authToken = context.ValidatedRequest?.Raw["token"];

            if(authToken != null)
            {
                var userInfo = await GetGoogleUserInfo(authToken);
                
                context.IssuedClaims.AddRange(new List<Claim>()
                {
                    new Claim("profile", userInfo.Email),
                    new Claim("name", userInfo.Name),
                    new Claim("picture", userInfo.Picture)
                });
            }
        }

        public async Task IsActiveAsync(IsActiveContext context)
        {
            // WE DO NOT USE THIS AS WE RELY ON GOOGLE AUTH
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
