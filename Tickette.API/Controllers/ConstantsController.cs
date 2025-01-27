using Microsoft.AspNetCore.Mvc;
using Tickette.Application.Wrappers;
using Tickette.Domain.Enums;

namespace Tickette.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ConstantsController : ControllerBase
{
    [HttpGet("approval-statuses")]
    public ResponseDto<string[]> GetApprovalStatuses()
    {
        var statuses = Enum.GetNames(typeof(ApprovalStatus));
        return ResponseHandler.SuccessResponse(statuses, "Retrieve Status Constant Successfully");
    }
}