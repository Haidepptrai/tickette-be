using Tickette.Domain.Entities;

namespace Tickette.Domain.Common;

public abstract class BaseAuditableEntity
{
    public Guid? CreatedByUserId { get; set; }
    public User CreatedByUser { get; set; }

    public Guid? UpdatedByUserId { get; set; }
    public User UpdatedByUser { get; set; }

    public Guid? DeletedByUserId { get; set; }
    public User DeletedByUser { get; set; }

    public DateTime CreatedDate { get; set; }
    public DateTime? UpdatedDate { get; set; }
    public DateTime? DeletedDate { get; set; }
}