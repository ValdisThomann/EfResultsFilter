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

    public override EntityQueryModelVisitor Create(QueryCompilationContext compilationContext, EntityQueryModelVisitor parentModelVisitor)
    {
        return new CustomRelationalQueryModelVisitor(
            Dependencies,
            RelationalDependencies,
            (RelationalQueryCompilationContext)compilationContext,
            (RelationalQueryModelVisitor)parentModelVisitor);
    }
}