using Microsoft.EntityFrameworkCore.Query;

class VisitorFactory :
    RelationalQueryModelVisitorFactory
{
    public VisitorFactory(
        EntityQueryModelVisitorDependencies dependencies,
        RelationalQueryModelVisitorDependencies relationalDependencies) :
        base(dependencies, relationalDependencies)
    {
    }

    public override EntityQueryModelVisitor Create(QueryCompilationContext compilationContext, EntityQueryModelVisitor parentModelVisitor)
    {
        return new ModelVisitor(
            Dependencies,
            RelationalDependencies,
            (RelationalQueryCompilationContext)compilationContext,
            (RelationalQueryModelVisitor)parentModelVisitor);
    }
}