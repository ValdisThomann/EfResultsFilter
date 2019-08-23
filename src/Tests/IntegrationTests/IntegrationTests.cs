using System.Threading.Tasks;
using EfFilter;
using EfLocalDb;
using Microsoft.EntityFrameworkCore;
using Xunit;
using Xunit.Abstractions;

public class IntegrationTests :
    XunitLoggingBase
{
    static SqlInstance<EfFilterDbContext> sqlInstance;

    static IntegrationTests()
    {
        sqlInstance = new SqlInstance<EfFilterDbContext>(
            buildTemplate: async dbContext =>
            {
                await dbContext.Database.EnsureCreatedAsync();
                await dbContext.Database.ExecuteSqlCommandAsync(
                    @"create view ParentEntityView as
        select Property
        from ParentEntities");
            },
            constructInstance: builder => new EfFilterDbContext(builder.Options));
    }

    public IntegrationTests(ITestOutputHelper output) :
        base(output)
    {
    }

    [Fact]
    public async Task Async()
    {
        var parent1 = new ParentEntity
        {
            Property = "Value1"
        };
        var child1 = new ChildEntity
        {
            Property = "Ignore",
            Parent = parent1
        };
        var child2 = new ChildEntity
        {
            Property = "Value2",
            Parent = parent1
        };
        var parent2 = new ParentEntity
        {
            Property = "Ignore"
        };
        var child3 = new ChildEntity
        {
            Property = "Value3",
            Parent = parent1
        };
        parent1.Children.Add(child1);
        parent1.Children.Add(child2);
        parent2.Children.Add(child3);
        using (var database = await sqlInstance.Build())
        {
            await database.AddDataUntracked(parent1, parent2);
            var result = await database.Context.ParentEntities.Include(x=>x.Children).ToListAsync();
            ObjectApprover.Verify(result);
        }
    }

    static GlobalFilters BuildFilters()
    {
        var filters = new GlobalFilters();
        filters.Add<ParentEntity>(item => item.Property != "Ignore");
        filters.Add<ChildEntity>(item => item.Property != "Ignore");
        return filters;
    }
}