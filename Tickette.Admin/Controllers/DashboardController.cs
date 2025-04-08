using Microsoft.AspNetCore.Mvc;
using Tickette.Application.Common.CQRS;
using Tickette.Application.Features.Dashboard.Queries.GetDashboardStatistics;
using Tickette.Application.Features.Dashboard.Queries.GetRevenueStatistics;
using Tickette.Application.Features.Events.Common.Admin;
using Tickette.Application.Features.Events.Queries.Admin.GetEventsStatistic;
using Tickette.Application.Wrappers;

namespace Tickette.Admin.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class DashboardController : ControllerBase
    {
        private readonly IQueryDispatcher _queryDispatcher;

        public DashboardController(IQueryDispatcher queryDispatcher)
        {
            _queryDispatcher = queryDispatcher;
        }

        [HttpGet("summary")]
        public async Task<ResponseDto<DashboardSummaryDto>> GetDashboardSummary(CancellationToken cancellationToken)
        {
            var query = new GetDashboardSummaryQuery();
            var result = await _queryDispatcher.Dispatch<GetDashboardSummaryQuery, DashboardSummaryDto>(query, cancellationToken);

            return ResponseHandler.SuccessResponse(result, "Dashboard summary retrieved successfully");
        }

        [HttpGet("revenue")]
        public async Task<ResponseDto<RevenueStatisticsDto>> GetRevenueStatistics([FromQuery] GetRevenueStatisticsQuery query, CancellationToken cancellationToken)
        {
            var result = await _queryDispatcher.Dispatch<GetRevenueStatisticsQuery, RevenueStatisticsDto>(query, cancellationToken);

            return ResponseHandler.SuccessResponse(result, "Revenue statistics retrieved successfully");
        }

        [HttpGet("events")]
        public async Task<ResponseDto<EventsStatisticDto>> GetEventStatistics(CancellationToken cancellationToken)
        {
            var query = new GetEventsStatisticQuery();
            var result = await _queryDispatcher.Dispatch<GetEventsStatisticQuery, EventsStatisticDto>(query, cancellationToken);

            return ResponseHandler.SuccessResponse(result, "Event statistics retrieved successfully");
        }
    }
}
