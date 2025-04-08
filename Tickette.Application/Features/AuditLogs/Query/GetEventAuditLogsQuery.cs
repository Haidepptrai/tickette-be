using Microsoft.EntityFrameworkCore;
using Tickette.Application.Common.CQRS;
using Tickette.Application.Common.Interfaces;
using Tickette.Application.Features.AuditLogs.Common;
using Tickette.Application.Wrappers;

namespace Tickette.Application.Features.AuditLogs.Query;

public record GetEventAuditLogsQuery
{
    public int PageNumber { get; init; } = 0;
    public int PageSize { get; init; } = 10;
    public string Type { get; init; }
}

public class GetEventAuditLogsQueryHandler : IQueryHandler<GetEventAuditLogsQuery, PagedResult<EventAuditLogDto>>
{
    private readonly IApplicationDbContext _context;

    public GetEventAuditLogsQueryHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<PagedResult<EventAuditLogDto>> Handle(GetEventAuditLogsQuery query, CancellationToken cancellationToken)
    {
        // Step 1: Build the query
        var sqlQuery = _context.AuditLogs
            .Where(al => al.TableName.Contains(query.Type))
            .OrderByDescending(al => al.Timestamp);

        // Step 2: Count total logs
        var totalCount = await sqlQuery.CountAsync(cancellationToken);

        // Step 3: Apply pagination directly in the DB
        var pagedAuditLogs = await sqlQuery
            .Skip(query.PageNumber * query.PageSize)
            .Take(query.PageSize)
            .ToListAsync(cancellationToken);

        // Step 4: Extract EventIds
        var eventIds = pagedAuditLogs
            .Select(al => al.EntityId)
            .Where(id => id != Guid.Empty)
            .Distinct()
            .ToList();

        // Step 5: Fetch matching Event names
        var eventNames = await _context.Events
            .Where(e => eventIds.Contains(e.Id))
            .ToDictionaryAsync(e => e.Id, e => e.Name, cancellationToken);

        // Step 6: Map into DTO
        var auditLogsDto = pagedAuditLogs.Select(al =>
        {
            eventNames.TryGetValue(al.EntityId, out var eventName);

            return new EventAuditLogDto
            {
                Id = al.Id,
                EntityName = eventName ?? "(deleted or unknown)",
                EntityId = al.EntityId,
                Action = al.Action,
                UserId = al.UserId,
                UserEmail = al.UserEmail,
                Data = al.Data,
                Timestamp = al.Timestamp
            };
        }).ToList();

        // Step 7: Return paged result
        return new PagedResult<EventAuditLogDto>(
            items: auditLogsDto,
            totalCount: totalCount,
            pageNumber: query.PageNumber,
            pageSize: query.PageSize
        );
    }

}