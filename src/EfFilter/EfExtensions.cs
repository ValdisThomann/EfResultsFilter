﻿using System;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Query;

namespace EfFilter
{
    public static class EfExtensions
    {
        public static void AddFilters<TDbContext>(this DbContextOptionsBuilder<TDbContext> builder)
            where TDbContext : DbContext
        {
            Guard.AgainstNull(nameof(builder), builder);
            builder.ReplaceService<IEntityQueryModelVisitorFactory, CustomRelationalQueryModelVisitorFactory>();
        }

        public static IDisposable StartFilteredQuery<TDbContext>(this TDbContext context, Filters filters)
            where TDbContext : DbContext
        {
            Guard.AgainstNull(nameof(context), context);
            CustomModelExpressionApplyingExpressionVisitor.filters.Value = filters;
            return new FilterCleaner();
        }
    }
}