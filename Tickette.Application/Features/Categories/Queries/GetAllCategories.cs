using Microsoft.EntityFrameworkCore;
using Tickette.Application.Common.CQRS;
using Tickette.Application.Common.Interfaces;
using Tickette.Domain.Entities;

namespace Tickette.Application.Features.Categories.Queries;

public record GetAllCategories
{

}

public class GetAllCategoriesHandler : IQueryHandler<GetAllCategories, IEnumerable<Category>>
{
    private readonly IApplicationDbContext _categoryRepository;

    public GetAllCategoriesHandler(IApplicationDbContext categoryRepository)
    {
        _categoryRepository = categoryRepository;
    }

    public async Task<IEnumerable<Category>> Handle(GetAllCategories query, CancellationToken cancellation)
    {
        var categories = await _categoryRepository.Categories.ToListAsync(cancellation);
        return categories;
    }
}