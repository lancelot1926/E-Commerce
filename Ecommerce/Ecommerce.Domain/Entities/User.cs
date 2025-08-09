using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Ecommerce.Domain.Common;
using Ecommerce.Domain.ValueObjects;

namespace Ecommerce.Domain.Entities;

public class User : BaseEntity
{
    // Basic info
    public string Name { get; set; } = string.Empty;
    public string Surname { get; set; } = string.Empty;

    // Contact & login
    public string Email { get; set; } = string.Empty;
    public string PhoneNumber { get; set; } = string.Empty;

    // Security (we will never store plaintext; infra will handle hashing)
    public string PasswordHash { get; set; } = string.Empty;

    // Address
    public Address? Address { get; set; }

    // Convenience full name
    public string FullName => $"{Name} {Surname}".Trim();

    // Simple helpers
    public void UpdatePhone(string phone)
    {
        PhoneNumber = phone;
        UpdatedAt = DateTime.UtcNow;
    }

    public void SetAddress(Address address)
    {
        Address = address;
        UpdatedAt = DateTime.UtcNow;
    }
}