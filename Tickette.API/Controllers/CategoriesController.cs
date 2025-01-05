using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Annotations;
using Tickette.Application.Common.CQRS;
using Tickette.Application.Features.Categories.Command.CreateCategory;
using Tickette.Application.Features.Categories.Command.DeleteCategory;
using Tickette.Application.Features.Categories.Command.UpdateCategory;
using Tickette.Application.Features.Categories.Queries;
using Tickette.Application.Helpers;
using Tickette.Domain.Entities;

namespace Tickette.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class CategoriesController : ControllerBase
    {
        private readonly IQueryDispatcher _queryDispatcher;
        private readonly ICommandDispatcher _commandDispatcher;

        public CategoriesController(IQueryDispatcher queryDispatcher, ICommandDispatcher commandDispatcher)
        {
            _queryDispatcher = queryDispatcher;
            _commandDispatcher = commandDispatcher;
        }

        // GET: api/Categories
        [HttpGet]
        public async Task<ResponseDto<IEnumerable<Category>>> GetAllCategories(CancellationToken cancellation)
        {
            var query = new GetAllCategories();
            var result = await _queryDispatcher.Dispatch<GetAllCategories, IEnumerable<Category>>(query, cancellation);

            return ResponseHandler.SuccessResponse(result);
        }

        // POST: api/Categories
        [HttpPost]
        [Authorize(Roles = "Admin")]
        [SwaggerOperation(
            Summary = "Create Category",
            Description = "Create a new Category to the system, required a Name"
        )]
        public async Task<ResponseDto<Category>> CreateCategory([FromBody] CreateCategoryCommand command, CancellationToken cancellation)
        {
            var result = await _commandDispatcher.Dispatch<CreateCategoryCommand, Category>(command, cancellation);

            return ResponseHandler.SuccessResponse(result);
        }

        // PUT: api/Categories/guid
        [HttpPut("{id:guid}")]
        [Authorize(Roles = "Admin")]
        public async Task<object> UpdateCategory(Guid id, [FromBody] UpdateCategoryCommand command, CancellationToken cancellation)
        {
            // Dispatch the update command
            var result = await _commandDispatcher.Dispatch<UpdateCategoryCommand, Category>(command, cancellation);

            // Return a success response
            return ResponseHandler.SuccessResponse(result);
        }

        // DELETE: api/Categories/guid
        [HttpDelete("{id:guid}")]
        [Authorize(Roles = "Admin")]
        public async Task<object> DeleteCategory(Guid id, CancellationToken cancellation)
        {
            // Dispatch the delete command
            object result = await _commandDispatcher.Dispatch<DeleteCategoryCommand, object>(new DeleteCategoryCommand { Id = id }, cancellation);

            // Return a success response
            return ResponseHandler.SuccessResponse(result, "Category Deleted");
        }

    }
}
