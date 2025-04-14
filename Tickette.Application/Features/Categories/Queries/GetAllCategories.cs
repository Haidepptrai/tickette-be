using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using Tickette.Application.Common.Constants;
using Tickette.Application.Common.CQRS;
using Tickette.Application.Common.Interfaces;
using Tickette.Application.Features.Events.Common;

namespace Tickette.Application.Features.Categories.Queries;

public record GetAllCategories;

public class GetAllCategoriesHandler : IQueryHandler<GetAllCategories, IEnumerable<CategoryDto>?>
{
    private readonly IApplicationDbContext _context;
    private readonly IMemoryCache _memoryCache;

    public GetAllCategoriesHandler(IApplicationDbContext context, IMemoryCache memoryCache)
    {
        _context = context;
        _memoryCache = memoryCache;
    }

    public async Task<IEnumerable<CategoryDto>?> Handle(GetAllCategories query, CancellationToken cancellation)
    {
        var categories = await _memoryCache.GetOrCreateAsync(InMemoryCacheKey.CategoryList(), async entry =>
        {
            entry.AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(5);
            return await _context.Categories
                .Select(c => new CategoryDto()
                {
                    Id = c.Id,
                    Name = c.Name,
                })
                .ToListAsync(cancellation);
        });

        return categories;
    }
}