using Tickette.Application.Common.CQRS;
using Tickette.Application.Common.Interfaces;
using Tickette.Domain.Entities;

namespace Tickette.Application.Features.Categories.Command.UpdateCategory;

public record UpdateCategoryCommand
{
    public Guid Id { get; set; }

    public string Name { get; set; }
}

public class UpdateCategoryCommandHandler : ICommandHandler<UpdateCategoryCommand, Category>
{
    private readonly IApplicationDbContext _context;

    public UpdateCategoryCommandHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<Category> Handle(UpdateCategoryCommand request, CancellationToken cancellationToken)
    {
        var category = await _context.Categories.FindAsync([request.Id], cancellationToken);

        if (category == null)
            throw new KeyNotFoundException("Category not found.");

        await _context.SaveChangesAsync(cancellationToken);

        return category;
    }
}