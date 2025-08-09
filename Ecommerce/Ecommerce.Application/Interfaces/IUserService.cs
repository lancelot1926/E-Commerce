using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Ecommerce.Application.Common;
using Ecommerce.Application.DTOs.Users;

namespace Ecommerce.Application.Interfaces;

public interface IUserService
{
    Task<Result<UserDto>> RegisterAsync(RegisterUserRequest request, CancellationToken ct = default);
    Task<Result<string>> LoginAsync(LoginRequest request, CancellationToken ct = default);
    // string will be a JWT later
    Task<Result<UserDto>> GetByIdAsync(int id, CancellationToken ct = default);
}
