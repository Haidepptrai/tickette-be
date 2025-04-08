using Microsoft.AspNetCore.Mvc;
using Tickette.Application.Common.CQRS;
using Tickette.Application.Features.AuditLogs.Common;
using Tickette.Application.Features.AuditLogs.Query;
using Tickette.Application.Wrappers;

namespace Tickette.Admin.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuditLogsController : ControllerBase
    {
        private readonly IQueryDispatcher _queryDispatcher;

        public AuditLogsController(IQueryDispatcher queryDispatcher)
        {
            _queryDispatcher = queryDispatcher;
        }

        [HttpPost("event")]
        public async Task<ActionResult<ResponseDto<EventAuditLogDto>>> GetEventAuditLogs([FromBody] GetEventAuditLogsQuery request, CancellationToken cancellationToken)
        {
            var result = await _queryDispatcher.Dispatch<GetEventAuditLogsQuery, PagedResult<EventAuditLogDto>>(request, cancellationToken);

            var paginationMeta = new PaginationMeta(request.PageNumber, request.PageSize, result.TotalCount, result.TotalPages);

            return Ok(ResponseHandler.PaginatedResponse(result.Items, paginationMeta, "Event Audit Logs retrieved successfully"));
        }
    }
}
