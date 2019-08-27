using System;

namespace EfFilter
{
    public class FilteredQuery<TEntity> :
        IDisposable
        where TEntity : class
    {
        public void Dispose()
        {
            CustomModelExpressionApplyingExpressionVisitor.filters.Value = null;
        }
    }
}