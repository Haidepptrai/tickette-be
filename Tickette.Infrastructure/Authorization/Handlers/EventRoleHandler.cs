using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.JsonWebTokens;
using Newtonsoft.Json.Linq;
using Tickette.Application.Common.Interfaces;
using Tickette.Infrastructure.Authorization.Requirements;

namespace Tickette.Infrastructure.Authorization.Handlers;

public class EventRoleHandler : AuthorizationHandler<EventRoleRequirement>
{
    private readonly IHttpContextAccessor _httpContextAccessor;
    private readonly IApplicationDbContext _context;

    public EventRoleHandler(IHttpContextAccessor httpContextAccessor, IApplicationDbContext context)
    {
        _httpContextAccessor = httpContextAccessor;
        _context = context;
    }

    protected override async Task HandleRequirementAsync(AuthorizationHandlerContext context, EventRoleRequirement requirement)
    {
        // Get UserId from claims
        var userId = Guid.Parse(context.User.FindFirst(JwtRegisteredClaimNames.Sub)?.Value ?? string.Empty);

        // Try to get the EventDateId from route or query string first
        var eventId = GetEventIdFromRouteOrQuery();

        // If not found, try to get it from the request body
        if (eventId == Guid.Empty)
        {
            eventId = await GetEventIdFromBodyAsync();
        }

        if (eventId == Guid.Empty)
        {
            Console.WriteLine("EventDateId not found");
            return;
        }

        // Check if the user has the required role for the specific event
        var hasRole = await _context.CommitteeMembers
            .AnyAsync(cm => cm.UserId == userId && cm.EventId == eventId && cm.CommitteeRole.Name == requirement.RequiredRole);

        if (hasRole)
        {
            context.Succeed(requirement);
        }
    }

    private Guid GetEventIdFromRouteOrQuery()
    {
        // Check route values
        var routeEventId = _httpContextAccessor.HttpContext?.Request.RouteValues["eventId"]?.ToString();
        if (Guid.TryParse(routeEventId, out var eventIdFromRoute))
        {
            return eventIdFromRoute;
        }

        // Check query string
        var queryEventId = _httpContextAccessor.HttpContext?.Request.Query["eventId"].ToString();
        if (Guid.TryParse(queryEventId, out var eventIdFromQuery))
        {
            return eventIdFromQuery;
        }

        return Guid.Empty;
    }

    private async Task<Guid> GetEventIdFromBodyAsync()
    {
        var request = _httpContextAccessor.HttpContext?.Request;

        if (request?.Body == null || !request.Body.CanRead)
        {
            return Guid.Empty;
        }

        // Enable buffering to allow multiple reads
        request.EnableBuffering();

        // Reset the stream position before reading
        request.Body.Seek(0, SeekOrigin.Begin);

        using var reader = new StreamReader(request.Body, leaveOpen: true);
        var bodyContent = await reader.ReadToEndAsync();

        // Reset the stream position after reading
        request.Body.Seek(0, SeekOrigin.Begin);

        if (string.IsNullOrWhiteSpace(bodyContent))
        {
            return Guid.Empty;
        }

        var json = JObject.Parse(bodyContent);
        var eventIdFromBody = json["event_id"]?.ToString();

        return Guid.TryParse(eventIdFromBody, out var eventId) ? eventId : Guid.Empty;
    }
}
