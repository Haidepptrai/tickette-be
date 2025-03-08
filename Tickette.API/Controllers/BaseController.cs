using Microsoft.AspNetCore.Mvc;
using System.IdentityModel.Tokens.Jwt;
using Tickette.Application.Exceptions;

namespace Tickette.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public abstract class BaseController : ControllerBase
    {
        protected string GetUserId()
        {
            var userIdClaim = User.Claims.FirstOrDefault(c => c.Type == JwtRegisteredClaimNames.Sub);
            if (userIdClaim == null || string.IsNullOrWhiteSpace(userIdClaim.Value))
            {
                throw new UserNotFoundException();
            }

            return userIdClaim.Value;
        }
    }
}
