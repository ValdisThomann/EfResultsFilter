using System.Reflection;
using Microsoft.EntityFrameworkCore.Query;

class ModelVisitor :
    RelationalQueryModelVisitor
{
    static FieldInfo field = typeof(EntityQueryModelVisitor)
        .GetField("_modelExpressionApplyingExpressionVisitor", BindingFlags.Instance | BindingFlags.NonPublic);

    public ModelVisitor(
        EntityQueryModelVisitorDependencies dependencies,
        RelationalQueryModelVisitorDependencies relationalDependencies,
        RelationalQueryCompilationContext context,
        RelationalQueryModelVisitor visitor) :
        base(dependencies, relationalDependencies, context, visitor)
    {
        var expressionVisitor = new ExpressionVisitor(
            context,
            dependencies.QueryModelGenerator,
            this);
        field.SetValue(this, expressionVisitor);
    }
}