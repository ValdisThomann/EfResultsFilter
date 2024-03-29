﻿using System.Threading.Tasks;
using EfResultFilter;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
// ReSharper disable UnusedVariable

public class GlobalFilterSnippets
{
    #region add-filter

    public class MyEntity
    {
        public string Property { get; set; }
    }

    #endregion

    public async Task Add(ServiceCollection services, MyDbContext myDbContext)
    {
        #region add-filter

        var filters = new Filters();
        filters.Add<MyEntity>(item => item.Property != "Ignore");
        using (myDbContext.FilterResults(filters))
        {
            var items = await myDbContext.MyEntities.ToListAsync();
        }

        #endregion
    }

    public class MyDbContext :
        DbContext
    {
        public DbSet<MyEntity> MyEntities { get; set; }
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<MyEntity>();
        }
    }
}