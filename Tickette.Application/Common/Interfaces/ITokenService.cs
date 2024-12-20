using Tickette.Domain.Entities;

namespace Tickette.Application.Common.Interfaces;

public interface ITokenService
{
    string GenerateToken(User user);
}