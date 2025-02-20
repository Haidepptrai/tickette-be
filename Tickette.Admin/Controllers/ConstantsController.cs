using Microsoft.AspNetCore.Mvc;
using Tickette.Application.Common.Interfaces;
using Tickette.Application.DTOs.Auth;
using Tickette.Application.Wrappers;
using Tickette.Domain.Enums;

namespace Tickette.Admin.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ConstantsController : ControllerBase
{
    private readonly IIdentityServices _identityServices;

    public ConstantsController(IIdentityServices identityServices)
    {
        _identityServices = identityServices;
    }

    [HttpGet("approval-statuses")]
    public ResponseDto<string[]> GetApprovalStatuses()
    {
        var statuses = Enum.GetNames(typeof(ApprovalStatus));
        return ResponseHandler.SuccessResponse(statuses, "Retrieve Status Constant Successfully");
    }

    [HttpPost("role-id")]
    public async Task<ActionResult<ResponseDto<RoleResponse[]>>> GetRoleIds()
    {
        var roleIds = await _identityServices.GetRoleIds();

        return Ok(ResponseHandler.SuccessResponse(roleIds, "Retrieve Role Id Constant Successfully"));
    }
}