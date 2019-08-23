using System.Threading.Tasks;
using EfFilter;
using Xunit;
using Xunit.Abstractions;

public class GlobalFiltersTests :
    XunitLoggingBase
{
    [Fact]
    public async Task Simple()
    {
        var filters= new Filters();
        filters.Add<Target>(target => target.Property != "Ignore");
        Assert.True(await filters.ShouldInclude(new Target()));
        Assert.False(await filters.ShouldInclude(null));
        Assert.True(await filters.ShouldInclude(new Target {Property = "Include"}));
        Assert.False(await filters.ShouldInclude(new Target {Property = "Ignore"}));

        filters.Add<BaseTarget>(target => target.Property != "Ignore");
        Assert.True(await filters.ShouldInclude(new ChildTarget()));
        Assert.True(await filters.ShouldInclude(new ChildTarget {Property = "Include"}));
        Assert.False(await filters.ShouldInclude(new ChildTarget {Property = "Ignore"}));

        filters.Add<ITarget>(target => target.Property != "Ignore");
        Assert.True(await filters.ShouldInclude( new ImplementationTarget()));
        Assert.True(await filters.ShouldInclude( new ImplementationTarget { Property = "Include"}));
        Assert.False(await filters.ShouldInclude(new ImplementationTarget { Property = "Ignore" }));

        Assert.True(await filters.ShouldInclude( new NonTarget { Property = "Foo" }));
    }

    public class NonTarget
    {
        public string Property { get; set; }
    }
    public class Target
    {
        public string Property { get; set; }
    }

    public class ChildTarget :
        BaseTarget
    {
    }

    public class BaseTarget
    {
        public string Property { get; set; }
    }

    public class ImplementationTarget :
        ITarget
    {
        public string Property { get; set; }
    }

    public interface ITarget
    {
        string Property { get; set; }
    }

    public GlobalFiltersTests(ITestOutputHelper output) :
        base(output)
    {
    }
}