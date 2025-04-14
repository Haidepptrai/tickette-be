using Microsoft.AspNetCore.Mvc;
using Tickette.Application.Common.CQRS;
using Tickette.Application.Features.Categories.Queries;
using Tickette.Application.Features.Events.Common;
using Tickette.Application.Wrappers;

namespace Tickette.API.Controllers;

[Route("api/[controller]")]
[ApiController]
public class CategoriesController : ControllerBase
{
    private readonly IQueryDispatcher _queryDispatcher;

    public CategoriesController(IQueryDispatcher queryDispatcher)
    {
        _queryDispatcher = queryDispatcher;
    }

    // GET: api/Categories
    [HttpGet]
    public async Task<ResponseDto<IEnumerable<CategoryDto>?>> GetAllCategories(CancellationToken cancellation)
    {
        var query = new GetAllCategories();
        var result = await _queryDispatcher.Dispatch<GetAllCategories, IEnumerable<CategoryDto>?>(query, cancellation);

        return ResponseHandler.SuccessResponse(result);
    }
}