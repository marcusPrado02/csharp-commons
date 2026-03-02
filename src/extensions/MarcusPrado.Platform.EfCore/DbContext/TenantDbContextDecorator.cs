using MarcusPrado.Platform.Abstractions.Context;
using Microsoft.EntityFrameworkCore;

namespace MarcusPrado.Platform.EfCore.DbContext;

/// <summary>
/// Extension methods for applying global tenant query filters to a <see cref="ModelBuilder"/>.
/// Filters all entities that expose a <c>TenantId</c> string property.
/// </summary>
public static class TenantDbContextDecorator
{
    /// <summary>
    /// Applies a global query filter on all entity types that have a
    /// <c>TenantId</c> property, restricting results to the current tenant.
    /// </summary>
    public static void ApplyTenantFilter(
        this ModelBuilder modelBuilder,
        ITenantContext tenantContext)
    {
        ArgumentNullException.ThrowIfNull(modelBuilder);
        ArgumentNullException.ThrowIfNull(tenantContext);

        foreach (var entityType in modelBuilder.Model.GetEntityTypes())
        {
            var tenantProp = entityType.FindProperty("TenantId");
            if (tenantProp is null || tenantProp.ClrType != typeof(string))
            {
                continue;
            }

            var parameter = System.Linq.Expressions.Expression.Parameter(
                entityType.ClrType, "e");
            var property = System.Linq.Expressions.Expression.Property(parameter, "TenantId");
            var tenantIdExpr = System.Linq.Expressions.Expression.Constant(tenantContext.TenantId);
            var equals = System.Linq.Expressions.Expression.Equal(property, tenantIdExpr);
            var lambda = System.Linq.Expressions.Expression.Lambda(equals, parameter);

            modelBuilder.Entity(entityType.ClrType).HasQueryFilter(lambda);
        }
    }
}
