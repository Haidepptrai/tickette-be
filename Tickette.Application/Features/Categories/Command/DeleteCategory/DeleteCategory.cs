using Tickette.Application.Common.CQRS;
using Tickette.Application.Common.Interfaces;

namespace Tickette.Application.Features.Categories.Command.DeleteCategory;

public record DeleteCategoryCommand
{
    public Guid Id { get; set; }
}

public class DeleteCategoryCommandHandler : ICommandHandler<DeleteCategoryCommand, object>
{
    private readonly IApplicationDbContext _context;

    public DeleteCategoryCommandHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<object> Handle(DeleteCategoryCommand request, CancellationToken cancellationToken)
    {
        var category = await _context.Categories.FindAsync(new object[] { request.Id }, cancellationToken);

        if (category == null)
            throw new KeyNotFoundException("Category not found.");

        _context.Categories.Remove(category);

        await _context.SaveChangesAsync(cancellationToken);

        return null!;
    }
}