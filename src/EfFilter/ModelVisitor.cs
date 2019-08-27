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
        RelationalQueryCompilationContext compilationContext,
        RelationalQueryModelVisitor modelVisitor) :
        base(dependencies, relationalDependencies, compilationContext, modelVisitor)
    {
        var expressionVisitor = new ExpressionVisitor(
            compilationContext,
            dependencies.QueryModelGenerator,
            this);
        field.SetValue(this, expressionVisitor);
    }
}