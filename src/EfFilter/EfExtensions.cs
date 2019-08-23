using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Query;
using Microsoft.EntityFrameworkCore.Query.Internal;

namespace EfFilter
{
    public static class EfExtensions
    {
        public static void AddFilters<TDbContext>(this DbContextOptionsBuilder<TDbContext> builder)
            where TDbContext : DbContext
        {
            Guard.AgainstNull(nameof(builder), builder);
            builder.ReplaceService<IQueryContextFactory, CustomQueryContextFactory>();
            builder.ReplaceService<IAsyncQueryProvider , CustomAsyncQueryProvider >();
        }

        public static IDisposable StartFilteredQuery<TDbContext>(this TDbContext context, Filters filters)
            where TDbContext : DbContext
        {
            Guard.AgainstNull(nameof(context), context);
            CustomQueryBuffer.filters.Value = filters;
            return new FilterCleaner();
        }

        //public static FilteredQuery<TEntity> Filtered<TEntity>(this DbSet<TEntity> context, Filters filters)
        //    where TEntity : class
        //{
        //    Guard.AgainstNull(nameof(context), context);
        //    CustomQueryBuffer.filters.Value = filters;
        //    return new FilteredQuery<TEntity>(context);
        //}
    }

    public class CustomAsyncQueryProvider:EntityQueryProvider
    {
        IQueryCompiler queryCompiler;

        public CustomAsyncQueryProvider(IQueryCompiler queryCompiler) : base(queryCompiler)
        {
            this.queryCompiler = queryCompiler;
        }
        public override IAsyncEnumerable<TResult> ExecuteAsync<TResult>(Expression expression)
        {
            var asyncEnumerable = queryCompiler.ExecuteAsync<TResult>(expression);
            return new CustomAsyncEnumerable<TResult>( asyncEnumerable);
        }

    }

    public class CustomAsyncEnumerable<T> : IAsyncEnumerable<T>
    {
        public CustomAsyncEnumerable(IAsyncEnumerable<object> asyncEnumerable)
        {

        }
    }
}