using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using EfFilter;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using Microsoft.EntityFrameworkCore.Query;
using Microsoft.EntityFrameworkCore.Query.Internal;

class CustomQueryBuffer :
    QueryBuffer
{
    internal static AsyncLocal<Filters> filters = new AsyncLocal<Filters>();
    Filters instanceFilters;

    public CustomQueryBuffer(QueryContextDependencies dependencies) :
        base(dependencies)
    {
        instanceFilters = filters.Value;
    }

    public override object GetEntity(IKey key, EntityLoadInfo entityLoadInfo, bool queryStateManager, bool throwOnNullKey)
    {
        var entity = base.GetEntity(key, entityLoadInfo, queryStateManager, throwOnNullKey);
        if (instanceFilters != null &&
            !instanceFilters.ShouldInclude(entity).GetAwaiter().GetResult())
        {
            return null;
        }

        return entity;
    }

    public override void IncludeCollection<TEntity, TRelated, TElement>(int includeId, INavigation navigation, INavigation inverseNavigation, IEntityType targetEntityType, IClrCollectionAccessor clrCollectionAccessor, IClrPropertySetter inverseClrPropertySetter, bool tracking, TEntity entity, Func<IEnumerable<TRelated>> relatedEntitiesFactory, Func<TEntity, TRelated, bool> joinPredicate)
    {
        base.IncludeCollection<TEntity, TRelated, TElement>(
            includeId,
            navigation,
            inverseNavigation,
            targetEntityType,
            clrCollectionAccessor,
            inverseClrPropertySetter,
            tracking,
            entity,
            () =>
            {
                var entitiesFactory = relatedEntitiesFactory();
                return entitiesFactory.Where(x => x != null);
            },
            joinPredicate);
    }

    public override Task IncludeCollectionAsync<TEntity, TRelated, TElement>(int includeId, INavigation navigation, INavigation inverseNavigation, IEntityType targetEntityType, IClrCollectionAccessor clrCollectionAccessor, IClrPropertySetter inverseClrPropertySetter, bool tracking, TEntity entity, Func<IAsyncEnumerable<TRelated>> relatedEntitiesFactory, Func<TEntity, TRelated, bool> joinPredicate, CancellationToken cancellationToken)
    {
        return base.IncludeCollectionAsync<TEntity, TRelated, TElement>(
            includeId,
            navigation,
            inverseNavigation,
            targetEntityType,
            clrCollectionAccessor,
            inverseClrPropertySetter,
            tracking,
            entity,
            () =>
            {
                var entitiesFactory = relatedEntitiesFactory();
                return entitiesFactory.Where(x => x != null);
            },
            joinPredicate,
            cancellationToken);
    }
}