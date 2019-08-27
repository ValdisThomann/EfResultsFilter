using System.Reflection;
using Microsoft.EntityFrameworkCore.Query;

class CustomRelationalQueryModelVisitor :
    RelationalQueryModelVisitor
{
    static FieldInfo field = typeof(EntityQueryModelVisitor)
        .GetField("_modelExpressionApplyingExpressionVisitor", BindingFlags.Instance | BindingFlags.NonPublic);

    public CustomRelationalQueryModelVisitor(
        EntityQueryModelVisitorDependencies dependencies,
        RelationalQueryModelVisitorDependencies relationalDependencies,
        RelationalQueryCompilationContext compilationContext,
        RelationalQueryModelVisitor modelVisitor) :
        base(dependencies, relationalDependencies, compilationContext, modelVisitor)
    {
        var expressionVisitor = new CustomModelExpressionApplyingExpressionVisitor(
            compilationContext,
            dependencies.QueryModelGenerator,
            this);
        field.SetValue(this, expressionVisitor);
    }
}