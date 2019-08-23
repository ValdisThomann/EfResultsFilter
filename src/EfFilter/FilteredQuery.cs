using System;

namespace EfFilter
{
    public class FilteredQuery<TEntity> :
        IDisposable
        where TEntity : class
    {

        public void Dispose()
        {
            CustomQueryBuffer.filters.Value = null;
        }

    }
}