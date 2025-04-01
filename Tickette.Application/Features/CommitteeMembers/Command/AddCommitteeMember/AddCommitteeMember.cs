using Microsoft.EntityFrameworkCore;
using Tickette.Application.Common.Constants;
using Tickette.Application.Common.CQRS;
using Tickette.Application.Common.Interfaces;
using Tickette.Application.Exceptions;
using Tickette.Domain.Entities;

namespace Tickette.Application.Features.CommitteeMembers.Command.AddCommitteeMember;

public record AddCommitteeMemberCommand
{
    public string MemberEmail { get; init; }
    public Guid RoleId { get; init; }
    public Guid EventId { get; init; }
}

public class AddCommitteeMemberCommandHandler : ICommandHandler<AddCommitteeMemberCommand, string>
{
    private readonly IApplicationDbContext _context;
    private readonly IIdentityServices _identityServices;
    private readonly ICacheService _cacheService;

    public AddCommitteeMemberCommandHandler(IApplicationDbContext context, IIdentityServices identityServices, ICacheService cacheService)
    {
        _context = context;
        _identityServices = identityServices;
        _cacheService = cacheService;
    }

    public async Task<string> Handle(AddCommitteeMemberCommand request, CancellationToken cancellationToken)
    {
        var user = await _identityServices.FindUserByEmailAsync(request.MemberEmail);

        if (user == null)
        {
            throw new NotFoundException("User", request.MemberEmail);
        }

        var alreadyExist = await _context.CommitteeMembers.AnyAsync(cm => cm.UserId == user.Id && cm.EventId == request.EventId, cancellationToken);

        if (alreadyExist)
        {
            throw new Exception("Member already exists in this event");
        }

        var entity = CommitteeMember.Create(user.Id, request.RoleId, request.EventId);

        _context.CommitteeMembers.Add(entity);

        await _context.SaveChangesAsync(cancellationToken);

        _cacheService.RemoveCacheValue(InMemoryCacheKey.CommitteeMemberOfEvent(request.EventId));

        return user.FullName!;
    }
}