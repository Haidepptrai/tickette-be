using Microsoft.AspNetCore.Mvc;
using Tickette.Application.Helpers;
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