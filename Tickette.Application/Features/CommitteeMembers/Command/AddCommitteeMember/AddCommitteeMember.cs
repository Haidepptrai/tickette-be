using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using System.Text.Json;
using Tickette.Application.Common;
using Tickette.Application.Common.Constants;
using Tickette.Application.Common.CQRS;
using Tickette.Application.Common.Interfaces;
using Tickette.Application.Common.Interfaces.Messaging;
using Tickette.Application.Common.Models.Email;
using Tickette.Application.Exceptions;
using Tickette.Application.Factories;
using Tickette.Application.Features.Events.Common.Client;
using Tickette.Domain.Entities;

namespace Tickette.Application.Features.CommitteeMembers.Command.AddCommitteeMember;

public record AddCommitteeMemberCommand
{
    public string MemberEmail { get; init; }
    public Guid RoleId { get; init; }
    public Guid EventId { get; init; }
}

public class AddCommitteeMemberCommandHandler : ICommandHandler<AddCommitteeMemberCommand, CommitteeMemberDto>
{
    private readonly IApplicationDbContext _context;
    private readonly ICacheService _cacheService;
    private readonly IMessageProducer _messageProducer;
    private readonly EmailSettings _emailSettings;

    public AddCommitteeMemberCommandHandler(IApplicationDbContext context, ICacheService cacheService, IMessageProducer messageProducer, IOptions<EmailSettings> emailSettings)
    {
        _context = context;
        _cacheService = cacheService;
        _messageProducer = messageProducer;
        _emailSettings = emailSettings.Value;
    }

    public async Task<CommitteeMemberDto> Handle(AddCommitteeMemberCommand request, CancellationToken cancellationToken)
    {
        var user = await _context.Users
            .SingleOrDefaultAsync(
                u => u.Email!.ToLower() == request.MemberEmail.ToLower(),
                cancellationToken);

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

        var eventInfo = _context.Events
            .SingleOrDefault(e => e.Id == request.EventId);

        var roleInfo = _context.CommitteeRoles.SingleOrDefault(r => r.Id == request.RoleId);

        var email = new AnnounceAddedMemberEmailModel(
            recipientName: user.FullName,
            recipientEmail: user.Email,
            eventName: eventInfo.Name,
            role: roleInfo.Name,
            eventLink: $"{_emailSettings.ClientUrl}/events/my-events/{eventInfo.Id}"
        );

        var wrapper = EmailWrapperFactory.Create(EmailServiceKeys.AnnounceAddedMember, email);
        var message = JsonSerializer.Serialize(wrapper);
        await _messageProducer.PublishAsync(EmailServiceKeys.Email, message);

        var result = new CommitteeMemberDto
        {
            UserId = user.Id,
            Email = user.Email,
            FullName = user.FullName,
            Role = roleInfo.Name,
        };

        return result;
    }
}