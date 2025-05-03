using Tickette.Domain.Entities;

namespace Tickette.Application.Common.Interfaces;

public interface ITokenService
{
    Task<string> GenerateToken(User user);

    string GenerateRefreshToken();
}