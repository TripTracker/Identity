using RestSharp.Deserializers;

namespace IdentityServer.Profiles
{
    public class UserProfile
    {
        public string Id { get; set; }
        public string Email { get; set; }
        public string Picture { get; set; }
        public string Locale { get; set; }

        [DeserializeAs(Name = "given_name")]
        public string FirstName { get; set; }

        [DeserializeAs(Name = "family_name")]
        public string LastName { get; set; }
    }
}
