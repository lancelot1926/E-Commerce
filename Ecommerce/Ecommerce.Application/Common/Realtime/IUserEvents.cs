namespace Ecommerce.Application.Common.Realtime;

public interface IUserEvents
{
    Task ForceLogoutAsync(int userId, string reason, CancellationToken ct = default);
}