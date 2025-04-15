namespace Tickette.Domain.Common;

public abstract class BaseSoftDeleteEntity
{
    public DateTime? DeletedAt { get; set; }

    public void SoftDeleteEntity()
    {
        DeletedAt = DateTime.UtcNow;
    }
}