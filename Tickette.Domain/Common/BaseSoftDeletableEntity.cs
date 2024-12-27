namespace Tickette.Domain.Common;

public abstract class BaseSoftDeletableEntity : BaseEntity
{
    public bool IsDeleted { get; set; }
    public DateTime? DeletedOn { get; set; }
    public string DeletedBy { get; set; }
}