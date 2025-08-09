using Microsoft.AspNetCore.Identity;
using System.Collections.Generic;
using System.Net;
namespace Ecommerce.Api.Models
{
    public class ApplicationUser : IdentityUser
    {
        public string FirstName { get; set; } = default!;
        public string LastName { get; set; } = default!;
        // IdentityUser already has: Id, Email, PasswordHash, PhoneNumber, etc.

        public ICollection<Address> Addresses { get; set; } = new List<Address>();
        public ShoppingCart? ShoppingCart { get; set; }
    }
}
