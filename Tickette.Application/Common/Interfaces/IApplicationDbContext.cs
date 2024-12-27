using Microsoft.EntityFrameworkCore;
using Tickette.Domain.Entities;

namespace Tickette.Application.Common.Interfaces;

public interface IApplicationDbContext
{
    DbSet<Event> Events { get; }

    DbSet<EventCommittee> EventCommittees { get; }

    DbSet<Ticket> Tickets { get; }

    DbSet<CommitteeMember> CommitteeMembers { get; }

    DbSet<Category> Categories { get; }

    Task<int> SaveChangesAsync(CancellationToken cancellationToken);
}