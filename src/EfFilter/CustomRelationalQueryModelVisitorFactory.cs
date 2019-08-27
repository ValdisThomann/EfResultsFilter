using Microsoft.EntityFrameworkCore.Query;

class CustomRelationalQueryModelVisitorFactory :
    RelationalQueryModelVisitorFactory
{
    public CustomRelationalQueryModelVisitorFactory(
        EntityQueryModelVisitorDependencies dependencies,
        RelationalQueryModelVisitorDependencies relationalDependencies) :
        base(dependencies, relationalDependencies)
    {
    }

    public override EntityQueryModelVisitor Create(QueryCompilationContext queryCompilationContext, EntityQueryModelVisitor parentEntityQueryModelVisitor)
    {
        return new CustomRelationalQueryModelVisitor(
            Dependencies,
            RelationalDependencies,
            (RelationalQueryCompilationContext)queryCompilationContext,
            (RelationalQueryModelVisitor)parentEntityQueryModelVisitor);
    }
}