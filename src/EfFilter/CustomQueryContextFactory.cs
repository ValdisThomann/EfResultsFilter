using Microsoft.EntityFrameworkCore.Query;
using Microsoft.EntityFrameworkCore.Query.Internal;
using Microsoft.EntityFrameworkCore.Storage;

class CustomQueryContextFactory : RelationalQueryContextFactory
{
    protected override IQueryBuffer CreateQueryBuffer()
    {
        return new CustomQueryBuffer(Dependencies);
    }

    public CustomQueryContextFactory(
        QueryContextDependencies dependencies,
        IRelationalConnection connection,
        IExecutionStrategyFactory strategyFactory) :
        base(dependencies, connection, strategyFactory)
    {
    }
}