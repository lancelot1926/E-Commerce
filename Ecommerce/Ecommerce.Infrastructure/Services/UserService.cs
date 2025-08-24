using Ecommerce.Application.Common;
using Ecommerce.Application.DTOs.Users;
using Ecommerce.Application.Interfaces;
using Ecommerce.Domain.Entities;
using Ecommerce.Domain.ValueObjects;
using Ecommerce.Infrastructure.Auth;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.AspNetCore.SignalR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Ecommerce.Application.Common.Realtime;

namespace Ecommerce.Infrastructure.Services;

public class UserService : IUserService
{
    private readonly IUserRepository _users;
    private readonly PasswordHasher<User> _hasher;
    private readonly IJwtTokenGenerator _jwt;
    private readonly IMemoryCache _cache;
    private readonly IUserEvents _userEvents;

    public UserService(IUserRepository users, PasswordHasher<User> hasher, IJwtTokenGenerator jwt, IMemoryCache cache,IUserEvents userEvents)
    {
        _users = users;
        _hasher = hasher;
        _jwt = jwt;
        _cache = cache;
        _userEvents = userEvents;
    }

    public async Task<Result<UserDto>> RegisterAsync(RegisterUserRequest r, CancellationToken ct = default)
    {
        var existing = await _users.GetByEmailAsync(r.Email, ct);
        if (existing is not null) return Result<UserDto>.Fail("Email already registered.");

        var user = new User
        {
            Name = r.Name,
            Surname = r.Surname,
            Email = r.Email,
            PhoneNumber = r.PhoneNumber,
            Role="User",
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

        user.PasswordHash = _hasher.HashPassword(user, r.Password);

        var id = await _users.AddAsync(user, ct);
        user.Id = id;

        return Result<UserDto>.Ok(user.ToDto());
    }

    public async Task<Result<string>> LoginAsync(LoginRequest request, CancellationToken ct = default)
    {
        var user = await _users.GetByEmailAsync(request.Email, ct);
        if (user is null) return Result<string>.Fail("Invalid credentials.");

        var verify = _hasher.VerifyHashedPassword(user, user.PasswordHash, request.Password);
        if (verify == PasswordVerificationResult.Failed) return Result<string>.Fail("Invalid credentials.");

        var token = _jwt.Generate(user);
        if (user.IsBanned) return Result<string>.Fail("Account is banned.");


        return Result<string>.Ok(token);
    }

    public async Task<Result<UserDto>> GetByIdAsync(int id, CancellationToken ct = default)
    {
        var user = await _users.GetByIdAsync(id, ct);
        if (user is null) return Result<UserDto>.Fail("User not found.");
        return Result<UserDto>.Ok(user.ToDto());
    }

    public async Task<Result<IReadOnlyList<UserDto>>>GetAllAsync(CancellationToken ct = default)
    {
        var users = await _users.GetAllAsync(ct);
        var dtos = users.Select(u => u.ToDto()).ToList();
        return Result<IReadOnlyList<UserDto>>.Ok(dtos);
    }

    public async Task<Result<UserDto>> BanAsync(int id, bool banned, CancellationToken ct = default)
    {
        var user = await _users.GetByIdAsync(id,ct);
        if (user is null) return Result<UserDto>.Fail("User not found.");
        if (user.IsBanned == banned)
            return Result<UserDto>.Ok(user.ToDto());

        user.IsBanned = banned;
        user.TokenVersion++;
        await _users.UpdateAsync(user, ct);
        _cache.Remove($"uv:{id}");
        await _userEvents.ForceLogoutAsync(id, "Your account was banned", ct);
        return Result<UserDto>.Ok(user.ToDto());
    }
}
