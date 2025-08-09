using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ecommerce.Domain.ValueObjects;

public class Address
{
    // Keep it simple; we’ll map it as an owned type in EF later.
    public string Line1 { get; set; } = string.Empty;   // Street & number
    public string? Line2 { get; set; }                  // Apartment, suite…
    public string City { get; set; } = string.Empty;
    public string State { get; set; } = string.Empty;   // or Province
    public string PostalCode { get; set; } = string.Empty;
    public string Country { get; set; } = string.Empty;
}