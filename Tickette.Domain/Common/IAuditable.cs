namespace Tickette.Domain.Common;

/// <summary>
/// Marker interface to flag entities that should be audit-logged via EF Core interceptor.
/// Only entities implementing this will be tracked.
/// </summary>
public interface IAuditable
{

}