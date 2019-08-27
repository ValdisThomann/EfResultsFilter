using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using EfLocalDb;
using EfResultFilter;
using Microsoft.EntityFrameworkCore;
using Xunit;
using Xunit.Abstractions;

public class IntegrationTests :
    XunitLoggingBase
{
    static SqlInstance<EfFilterDbContext> sqlInstance;

    static IntegrationTests()
    {
        LocalDbLogging.EnableVerbose(sqlLogging: true);
        sqlInstance = new SqlInstance<EfFilterDbContext>(
            buildTemplate: async dbContext =>
            {
                await dbContext.Database.EnsureCreatedAsync();
                await dbContext.Database.ExecuteSqlCommandAsync(
                    @"create view ParentEntityView as
        select Property
        from ParentEntities");
            },
            constructInstance: builder =>
            {
                builder.AddFilters();
                return new EfFilterDbContext(builder.Options);
            });
    }

    public IntegrationTests(ITestOutputHelper output) :
        base(output)
    {
    }

    [Fact]
    public async Task Sync()
    {
        using (var database = await BuildContext())
        using (database.Context.StartFilteredQuery(BuildFilters()))
        {
            var result = database.Context.ParentEntities.Include(x => x.Children).ToList();
            ObjectApprover.Verify(result);
        }
    }

    [Fact]
    public async Task SyncSingle()
    {
        using (var database = await BuildContext())
        using (database.Context.StartFilteredQuery(BuildFilters()))
        {
            var result = database.Context.ParentEntities.Include(x => x.Children).Single();
            ObjectApprover.Verify(result);
        }
    }

    [Fact]
    public async Task AsyncSingle()
    {
        using (var database = await BuildContext())
        using (database.Context.StartFilteredQuery(BuildFilters()))
        {
            var result = await database.Context.ParentEntities.Include(x => x.Children).SingleAsync();
            ObjectApprover.Verify(result);
        }
    }

    [Fact]
    public async Task Async()
    {
        using (var database = await BuildContext())
        using (database.Context.StartFilteredQuery(BuildFilters()))
        {
            var result = await database.Context.ParentEntities.Include(x => x.Children).ToListAsync();
            ObjectApprover.Verify(result);
        }
    }

    [Fact]
    public async Task WhereBefore()
    {
        var parent1 = new ParentEntity
        {
            Property = "Value1"
        };
        var parent2 = new ParentEntity
        {
            Property = "Ignore"
        };
        var parent3 = new ParentEntity
        {
            Property = "OtherIgnore"
        };
        var database1 = await sqlInstance.Build();
        await database1.AddDataUntracked(parent1, parent2, parent3);
        using (var database = database1)
        using (database.Context.StartFilteredQuery(BuildFilters()))
        {
            var result = await database.Context.ParentEntities.Where(x => x.Property != "OtherIgnore").ToListAsync();
            ObjectApprover.Verify(result);
        }
    }

    async Task<SqlDatabase<EfFilterDbContext>> BuildContext(
        [CallerMemberName] string memberName = null)
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
        var database = await sqlInstance.Build(memberName);
        await database.AddDataUntracked(parent1, parent2);
        return database;
    }

    static Filters BuildFilters()
    {
        var filters = new Filters();
        filters.Add<ParentEntity>(item => item.Property != "Ignore");
        filters.Add<ChildEntity>(item => item.Property != "Ignore");
        return filters;
    }
}