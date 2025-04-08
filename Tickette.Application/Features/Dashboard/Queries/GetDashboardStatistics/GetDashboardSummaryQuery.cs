using Microsoft.EntityFrameworkCore;
using Tickette.Application.Common.CQRS;
using Tickette.Application.Common.Interfaces;
using Tickette.Domain.Enums;

namespace Tickette.Application.Features.Dashboard.Queries.GetDashboardStatistics;

public record GetDashboardSummaryQuery { }

public class GetDashboardSummaryQueryHandler : IQueryHandler<GetDashboardSummaryQuery, DashboardSummaryDto>
{
    private readonly IApplicationDbContext _context;

    public GetDashboardSummaryQueryHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<DashboardSummaryDto> Handle(GetDashboardSummaryQuery query, CancellationToken cancellationToken)
    {
        var now = DateTime.UtcNow;
        var today = DateTime.UtcNow.Date;
        var yesterday = today.AddDays(-1);
        var sevenDaysAgo = today.AddDays(-7);
        var thirtyDaysAgo = today.AddDays(-30);

        // Total Users
        var totalUsers = await _context.Users.CountAsync(cancellationToken);

        // Total Events
        var totalEvents = await _context.Events.CountAsync(cancellationToken);

        // Active Events (approved and not ended)
        var activeEvents = await _context.Events
            .Where(e => e.Status == ApprovalStatus.Approved)
            .Where(e => e.EventDates.Any(ed => ed.EndDate > now))
            .CountAsync(cancellationToken);

        // Total Orders
        var totalOrders = await _context.Orders.CountAsync(cancellationToken);

        // Orders Today
        var ordersToday = await _context.Orders
            .Where(o => o.CreatedAt >= today)
            .CountAsync(cancellationToken);

        // Total Revenue
        var totalRevenue = await _context.Orders
            .SumAsync(o => o.TotalPrice, cancellationToken);

        // Revenue Today
        var revenueToday = await _context.Orders
            .Where(o => o.CreatedAt >= today)
            .SumAsync(o => o.TotalPrice, cancellationToken);

        // Revenue This Week
        var revenueThisWeek = await _context.Orders
            .Where(o => o.CreatedAt >= sevenDaysAgo)
            .SumAsync(o => o.TotalPrice, cancellationToken);

        // Revenue This Month
        var revenueThisMonth = await _context.Orders
            .Where(o => o.CreatedAt >= thirtyDaysAgo)
            .SumAsync(o => o.TotalPrice, cancellationToken);

        // Pending Events Count
        var pendingEvents = await _context.Events
            .CountAsync(e => e.Status == ApprovalStatus.Pending, cancellationToken);



        return new DashboardSummaryDto
        {
            TotalUsers = totalUsers,
            TotalEvents = totalEvents,
            ActiveEvents = activeEvents,
            TotalOrders = totalOrders,
            OrdersToday = ordersToday,
            TotalRevenue = totalRevenue,
            RevenueToday = revenueToday,
            RevenueThisWeek = revenueThisWeek,
            RevenueThisMonth = revenueThisMonth,
            PendingEvents = pendingEvents,
        };
    }
}

public record DashboardSummaryDto
{
    public int TotalUsers { get; init; }
    public int TotalEvents { get; init; }
    public int ActiveEvents { get; init; }
    public int TotalOrders { get; init; }
    public int OrdersToday { get; init; }
    public decimal TotalRevenue { get; init; }
    public decimal RevenueToday { get; init; }
    public decimal RevenueThisWeek { get; init; }
    public decimal RevenueThisMonth { get; init; }
    public int PendingEvents { get; init; }
    public int NewUsersToday { get; init; }
    public int NewUsersThisWeek { get; init; }
}