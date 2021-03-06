using Microsoft.AspNetCore.Identity;

namespace IdentityServer.Context
{
    public class TripTreckerUser : IdentityUser
    {
        public string Picture { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
    }
}
