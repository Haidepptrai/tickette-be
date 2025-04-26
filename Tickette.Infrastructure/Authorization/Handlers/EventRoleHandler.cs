using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using System.IdentityModel.Tokens.Jwt;
using System.Text.Json;
using Tickette.Application.Common.Interfaces;
using Tickette.Infrastructure.Authorization.Requirements;

namespace Tickette.Infrastructure.Authorization.Handlers;

public class EventRoleHandler : AuthorizationHandler<EventRoleRequirement>
{
    private readonly IServiceScopeFactory _serviceScopeFactory;
    private readonly IHttpContextAccessor _httpContextAccessor;

    public EventRoleHandler(IServiceScopeFactory serviceScopeFactory, IHttpContextAccessor httpContextAccessor)
    {
        _serviceScopeFactory = serviceScopeFactory;
        _httpContextAccessor = httpContextAccessor;
    }

    protected override async Task HandleRequirementAsync(
        AuthorizationHandlerContext context,
        EventRoleRequirement requirement)
    {
        var httpContext = _httpContextAccessor.HttpContext;
        if (httpContext == null)
        {
            context.Fail();
            return;
        }

        var userId = context.User.FindFirst(JwtRegisteredClaimNames.Sub)?.Value;
        if (string.IsNullOrEmpty(userId))
        {
            context.Fail();
            return;
        }

        var eventId = await ExtractEventIdFromRequestAsync(httpContext);
        if (string.IsNullOrEmpty(eventId))
        {
            context.Fail();
            return;
        }

        // **Create a Scoped Database Context**
        using var scope = _serviceScopeFactory.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<IApplicationDbContext>();

        // Fetch user role dynamically
        var userRoles = await dbContext.CommitteeMembers
            .Where(ec => ec.UserId.ToString() == userId && ec.EventId.ToString() == eventId)
            .Select(ec => ec.CommitteeRole.Name)
            .ToListAsync(httpContext.RequestAborted);


        if (userRoles.Any(role => requirement.RequiredRoles.Contains(role)))
        {
            context.Succeed(requirement);
        }
        else
        {
            context.Fail();
        }
    }

    private async Task<string?> ExtractEventIdFromRequestAsync(HttpContext httpContext)
    {
        var request = httpContext.Request;

        // 1. Check route parameters (e.g., /events/{eventId}/scan-qr)
        if (request.RouteValues.TryGetValue("eventId", out var routeEventId))
        {
            return routeEventId?.ToString();
        }

        // 2. Check query parameters (e.g., /scan-qr?eventId=123)
        if (request.Query.TryGetValue("eventId", out var queryEventId))
        {
            return queryEventId.ToString();
        }

        // 3. Check request body (for POST/PUT methods)
        if (request.Method == HttpMethods.Post || request.Method == HttpMethods.Put)
        {
            try
            {
                request.EnableBuffering(); // Allows re-reading request body
                using var reader = new StreamReader(request.Body, leaveOpen: true);
                var bodyContent = await reader.ReadToEndAsync();
                request.Body.Position = 0; // Reset stream position for middleware

                using var bodyJson = JsonDocument.Parse(bodyContent);
                if (bodyJson.RootElement.TryGetProperty("eventId", out var bodyEventId))
                {
                    return bodyEventId.GetString();
                }
            }
            catch (JsonException)
            {
                return null; // Malformed JSON
            }
        }

        return null; // No event ID found
    }
}
