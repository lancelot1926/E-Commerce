using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Ecommerce.Application.DTOs.Products;
using Ecommerce.Application.DTOs.Users;
using Ecommerce.Domain.Entities;
using Ecommerce.Domain.ValueObjects;

namespace Ecommerce.Application.Common;

public static class Mapping
{
    public static ProductDto ToDto(this Product p) => new()
    {
        Id = p.Id,
        Name = p.Name,
        Description = p.Description,
        Price = p.Price,
        StockQuantity = p.StockQuantity,
        Sku = p.Sku,
        Brand = p.Brand,
        Category = p.Category,
        MainImageUrl = p.MainImageUrl,
        IsActive = p.IsActive
    };

    public static UserDto ToDto(this User u) => new()
    {
        Id = u.Id,
        Name = u.Name,
        Surname = u.Surname,
        Email = u.Email,
        PhoneNumber = u.PhoneNumber,
        AddressLine1 = u.Address?.Line1,
        AddressLine2 = u.Address?.Line2,
        City = u.Address?.City,
        State = u.Address?.State,
        PostalCode = u.Address?.PostalCode,
        Country = u.Address?.Country
    };

    public static User FromRegister(this RegisterUserRequest r) => new()
    {
        Name = r.Name,
        Surname = r.Surname,
        Email = r.Email,
        PhoneNumber = r.PhoneNumber,
        Address = (r.AddressLine1 is null && r.City is null) ? null : new Address
        {
            Line1 = r.AddressLine1 ?? string.Empty,
            Line2 = r.AddressLine2,
            City = r.City ?? string.Empty,
            State = r.State ?? string.Empty,
            PostalCode = r.PostalCode ?? string.Empty,
            Country = r.Country ?? string.Empty
        }
    };
}