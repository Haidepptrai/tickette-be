using Microsoft.AspNetCore.Http;
using System.Security.Claims;
using Tickette.Application.Common.Interfaces;

namespace Tickette.Infrastructure.Authorization;

public class CurrentUserService : ICurrentUserService
{
    private readonly IHttpContextAccessor _httpContextAccessor;

    public CurrentUserService(IHttpContextAccessor httpContextAccessor)
    {
        _httpContextAccessor = httpContextAccessor;
    }

    public string? UserId => _httpContextAccessor.HttpContext?.User?.FindFirst(ClaimTypes.NameIdentifier)?.Value;

    public bool IsAdmin => _httpContextAccessor.HttpContext?.User?.IsInRole("Admin") ?? false;

    public bool IsModerator => _httpContextAccessor.HttpContext?.User.IsInRole("Moderator") ?? false;
}
