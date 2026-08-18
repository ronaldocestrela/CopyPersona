using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using PersonaScript.BuildingBlocks.Domain;

namespace PersonaScript.BuildingBlocks.Tenancy;

public sealed class TenantDbContextInterceptor(ITenantContext tenantContext) : SaveChangesInterceptor
{
    public override InterceptionResult<int> SavingChanges(DbContextEventData eventData, InterceptionResult<int> result)
    {
        UpdateTenantId(eventData.Context);
        return base.SavingChanges(eventData, result);
    }

    public override ValueTask<InterceptionResult<int>> SavingChangesAsync(
        DbContextEventData eventData,
        InterceptionResult<int> result,
        CancellationToken cancellationToken = default)
    {
        UpdateTenantId(eventData.Context);
        return base.SavingChangesAsync(eventData, result, cancellationToken);
    }

    private void UpdateTenantId(DbContext? context)
    {
        if (context is null)
        {
            return;
        }

        var currentTenantId = tenantContext.TenantId.Value;

        foreach (var entry in context.ChangeTracker.Entries<IMustHaveTenant>())
        {
            if (entry.State == EntityState.Added)
            {
                var tenantIdProperty = entry.Property(e => e.TenantId);
                var existingTenantId = tenantIdProperty.CurrentValue;

                if (existingTenantId == Guid.Empty)
                {
                    if (currentTenantId == Guid.Empty)
                    {
                        throw new InvalidOperationException(
                            $"Cannot insert entity '{entry.Entity.GetType().Name}' implementing IMustHaveTenant without an active authenticated TenantContext.");
                    }

                    tenantIdProperty.CurrentValue = currentTenantId;
                    entry.Entity.SetTenantId(currentTenantId);
                }
                else if (currentTenantId != Guid.Empty && existingTenantId != currentTenantId)
                {
                    throw new InvalidOperationException(
                        $"Cannot insert entity '{entry.Entity.GetType().Name}' with TenantId '{existingTenantId}' under active TenantContext '{currentTenantId}'.");
                }
            }
            else if (entry.State == EntityState.Modified)
            {
                var tenantIdProperty = entry.Property(e => e.TenantId);
                if (tenantIdProperty.IsModified && tenantIdProperty.OriginalValue != tenantIdProperty.CurrentValue)
                {
                    throw new InvalidOperationException(
                        $"Cannot modify TenantId for entity '{entry.Entity.GetType().Name}'. Cross-tenant modification is strictly prohibited.");
                }
            }
        }
    }
}
