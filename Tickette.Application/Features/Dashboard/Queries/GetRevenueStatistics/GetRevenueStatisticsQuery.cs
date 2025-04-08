using Microsoft.EntityFrameworkCore;
using Tickette.Application.Common.CQRS;
using Tickette.Application.Common.Interfaces;

namespace Tickette.Application.Features.Dashboard.Queries.GetRevenueStatistics;

public record GetRevenueStatisticsQuery
{
    public string TimeFrame { get; init; } = "weekly"; // "daily", "weekly", "monthly", "yearly"
    public int Count { get; init; } = 12; // Number of periods to return
}

public class GetRevenueStatisticsQueryHandler : IQueryHandler<GetRevenueStatisticsQuery, RevenueStatisticsDto>
{
    private readonly IApplicationDbContext _context;

    public GetRevenueStatisticsQueryHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<RevenueStatisticsDto> Handle(GetRevenueStatisticsQuery query, CancellationToken cancellationToken)
    {
        var now = DateTime.UtcNow;
        var periods = new List<RevenuePeriodDto>();

        // Default to weekly if not specified or invalid
        var timeFrame = query.TimeFrame.ToLowerInvariant() switch
        {
            "daily" => "daily",
            "weekly" => "weekly",
            "monthly" => "monthly",
            "yearly" => "yearly",
            _ => "weekly"
        };

        // Adjust count to reasonable limits
        var count = Math.Min(Math.Max(1, query.Count), 50);

        // Prepare query for all orders we'll need
        var ordersQuery = _context.Orders.AsNoTracking();

        switch (timeFrame)
        {
            case "daily":
                for (int i = 0; i < count; i++)
                {
                    var date = now.Date.AddDays(-i);
                    var nextDate = date.AddDays(1);

                    var periodRevenue = await ordersQuery
                        .Where(o => o.CreatedAt >= date && o.CreatedAt < nextDate)
                        .SumAsync(o => o.TotalPrice, cancellationToken);

                    var periodOrders = await ordersQuery
                        .Where(o => o.CreatedAt >= date && o.CreatedAt < nextDate)
                        .CountAsync(cancellationToken);

                    periods.Add(new RevenuePeriodDto
                    {
                        Period = date.ToString("yyyy-MM-dd"),
                        Label = date.ToString("MMM dd"),
                        Revenue = periodRevenue,
                        OrderCount = periodOrders
                    });
                }
                break;

            case "weekly":
                for (int i = 0; i < count; i++)
                {
                    var endDate = now.Date.AddDays(-(i * 7));
                    var startDate = endDate.AddDays(-6);

                    var periodRevenue = await ordersQuery
                        .Where(o => o.CreatedAt >= startDate && o.CreatedAt < endDate.AddDays(1))
                        .SumAsync(o => o.TotalPrice, cancellationToken);

                    var periodOrders = await ordersQuery
                        .Where(o => o.CreatedAt >= startDate && o.CreatedAt < endDate.AddDays(1))
                        .CountAsync(cancellationToken);

                    periods.Add(new RevenuePeriodDto
                    {
                        Period = $"{startDate:yyyy-MM-dd} to {endDate:yyyy-MM-dd}",
                        Label = $"{startDate:MMM dd} - {endDate:MMM dd}",
                        Revenue = periodRevenue,
                        OrderCount = periodOrders
                    });
                }
                break;

            case "monthly":
                for (int i = 0; i < count; i++)
                {
                    var date = new DateTime(now.Year, now.Month, 1).AddMonths(-i);
                    var nextMonth = date.AddMonths(1);

                    var periodRevenue = await ordersQuery
                        .Where(o => o.CreatedAt >= date && o.CreatedAt < nextMonth)
                        .SumAsync(o => o.TotalPrice, cancellationToken);

                    var periodOrders = await ordersQuery
                        .Where(o => o.CreatedAt >= date && o.CreatedAt < nextMonth)
                        .CountAsync(cancellationToken);

                    periods.Add(new RevenuePeriodDto
                    {
                        Period = date.ToString("yyyy-MM"),
                        Label = date.ToString("MMM yyyy"),
                        Revenue = periodRevenue,
                        OrderCount = periodOrders
                    });
                }
                break;

            case "yearly":
                for (int i = 0; i < count; i++)
                {
                    var date = new DateTime(now.Year, 1, 1).AddYears(-i);
                    var nextYear = date.AddYears(1);

                    var periodRevenue = await ordersQuery
                        .Where(o => o.CreatedAt >= date && o.CreatedAt < nextYear)
                        .SumAsync(o => o.TotalPrice, cancellationToken);

                    var periodOrders = await ordersQuery
                        .Where(o => o.CreatedAt >= date && o.CreatedAt < nextYear)
                        .CountAsync(cancellationToken);

                    periods.Add(new RevenuePeriodDto
                    {
                        Period = date.ToString("yyyy"),
                        Label = date.ToString("yyyy"),
                        Revenue = periodRevenue,
                        OrderCount = periodOrders
                    });
                }
                break;
        }

        // Calculate average order value
        decimal totalRevenue = periods.Sum(p => p.Revenue);
        int totalOrders = periods.Sum(p => p.OrderCount);
        decimal averageOrderValue = totalOrders > 0 ? totalRevenue / totalOrders : 0;

        return new RevenueStatisticsDto
        {
            TimeFrame = timeFrame,
            Periods = periods.OrderBy(p => p.Period).ToList(),
            TotalRevenue = totalRevenue,
            TotalOrders = totalOrders,
            AverageOrderValue = averageOrderValue
        };
    }
}

public record RevenueStatisticsDto
{
    public string TimeFrame { get; init; } = string.Empty;
    public List<RevenuePeriodDto> Periods { get; init; } = new();
    public decimal TotalRevenue { get; init; }
    public int TotalOrders { get; init; }
    public decimal AverageOrderValue { get; init; }
}

public record RevenuePeriodDto
{
    public string Period { get; init; } = string.Empty;
    public string Label { get; init; } = string.Empty;
    public decimal Revenue { get; init; }
    public int OrderCount { get; init; }
} 