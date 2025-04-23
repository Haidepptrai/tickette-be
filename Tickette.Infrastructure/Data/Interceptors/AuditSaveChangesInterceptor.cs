using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.Extensions.Logging;
using Microsoft.IdentityModel.JsonWebTokens;
using System.Security.Claims;
using System.Text.Json;
using Tickette.Domain.Common;
using Tickette.Domain.Entities;

namespace Tickette.Infrastructure.Data.Interceptors;

public class AuditSaveChangesInterceptor : SaveChangesInterceptor
{
    private readonly IHttpContextAccessor _httpContextAccessor;
    private readonly ILogger<AuditSaveChangesInterceptor> _logger;
    private readonly string[] _importantFields = ["Status", "Reason"];

    public AuditSaveChangesInterceptor(IHttpContextAccessor httpContextAccessor, ILogger<AuditSaveChangesInterceptor> logger)
    {
        _httpContextAccessor = httpContextAccessor;
        _logger = logger;
    }

    public override InterceptionResult<int> SavingChanges(DbContextEventData eventData, InterceptionResult<int> result)
    {
        UpdateEntities(eventData.Context);
        return base.SavingChanges(eventData, result);
    }

    public override ValueTask<InterceptionResult<int>> SavingChangesAsync(
        DbContextEventData eventData,
        InterceptionResult<int> result,
        CancellationToken cancellationToken = default)
    {
        UpdateEntities(eventData.Context);
        return base.SavingChangesAsync(eventData, result, cancellationToken);
    }

    private void UpdateEntities(DbContext? context)
    {
        if (context == null) return;

        if (!IsAdmin()) return;

        var (userId, userEmail) = GetCurrentUserInformation();
        if (userId == null || userEmail == null)
        {
            _logger.LogWarning("User information is not available.");
            return;
        }

        CreateAuditLogs(context, userId.Value, userEmail);
    }

    private void CreateAuditLogs(DbContext context, Guid userId, string userEmail)
    {
        var auditEntries = new List<AuditEntry>();

        foreach (var entry in context.ChangeTracker.Entries())
        {
            if (entry.Entity is AuditLog ||
                entry.State == EntityState.Detached ||
                entry.State == EntityState.Unchanged ||
                entry.Entity is not IAuditable)
                continue;

            var auditEntry = new AuditEntry
            {
                TableName = entry.Metadata.GetTableName() ?? entry.Entity.GetType().Name,
                EntityId = GetEntityId(entry),
                Action = entry.State.ToString(),
                Timestamp = DateTime.UtcNow,
                UserId = userId,
                Changes = new Dictionary<string, object>()
            };

            foreach (var property in entry.Properties)
            {
                var propertyName = property.Metadata.Name;

                // Filter: Only track important fields
                if (!_importantFields.Contains(propertyName))
                    continue;

                if (property.Metadata.IsKey() || property.Metadata.IsForeignKey() || !IsSupportedScalarType(property.Metadata.ClrType))
                    continue;

                if (entry.State == EntityState.Added)
                {
                    auditEntry.Changes[propertyName] = property.CurrentValue ?? DBNull.Value;
                }
                else if (entry.State == EntityState.Deleted)
                {
                    auditEntry.Changes[propertyName] = property.OriginalValue ?? DBNull.Value;
                }
                else if (entry.State == EntityState.Modified)
                {
                    if (!Equals(property.OriginalValue, property.CurrentValue))
                    {
                        auditEntry.Changes[propertyName] = new Dictionary<string, object?>
                        {
                            { "Old", property.OriginalValue },
                            { "New", property.CurrentValue }
                        };
                    }
                }
            }

            if (auditEntry.Changes.Any())
            {
                auditEntries.Add(auditEntry);
            }
        }

        foreach (var auditEntry in auditEntries)
        {
            var log = new AuditLog(
                entityId: auditEntry.EntityId,
                tableName: auditEntry.TableName,
                action: auditEntry.Action,
                timestamp: auditEntry.Timestamp,
                userId: auditEntry.UserId,
                userEmail: userEmail,
                data: JsonSerializer.Serialize(auditEntry.Changes)
            );

            context.Set<AuditLog>().Add(log);
        }
    }

    private Guid GetEntityId(EntityEntry entry)
    {
        var key = entry.Metadata.FindPrimaryKey()?.Properties.FirstOrDefault();
        var value = key != null ? entry.Property(key.Name).CurrentValue : null;

        return value is Guid guid ? guid : Guid.Empty;
    }

    private (Guid?, string?) GetCurrentUserInformation()
    {
        var userIdStr = _httpContextAccessor.HttpContext?.User.FindFirstValue(JwtRegisteredClaimNames.Sub);
        var userEmail = _httpContextAccessor.HttpContext?.User.FindFirstValue(JwtRegisteredClaimNames.Email);

        if (string.IsNullOrEmpty(userIdStr) || string.IsNullOrEmpty(userEmail))
            return (null, null);

        if (!Guid.TryParse(userIdStr, out var userId))
            return (null, null);

        return (userId, userEmail);
    }

    private bool IsAdmin()
    {
        var roles = _httpContextAccessor.HttpContext?.User.Claims
            .Where(c => c.Type == ClaimTypes.Role)
            .Select(c => c.Value);

        return roles != null && roles.Contains("Admin");
    }

    private static bool IsSupportedScalarType(Type type)
    {
        return type.IsPrimitive
               || type.IsEnum
               || type == typeof(string)
               || type == typeof(Guid)
               || type == typeof(decimal)
               || type == typeof(DateTime)
               || type == typeof(DateTimeOffset)
               || type == typeof(TimeSpan)
               || Nullable.GetUnderlyingType(type) is not null && IsSupportedScalarType(Nullable.GetUnderlyingType(type)!);
    }
}