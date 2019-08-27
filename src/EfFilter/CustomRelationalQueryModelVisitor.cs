using System.Reflection;
using Microsoft.EntityFrameworkCore.Query;

public class CustomRelationalQueryModelVisitor :
    RelationalQueryModelVisitor
{
    public CustomRelationalQueryModelVisitor(
        EntityQueryModelVisitorDependencies dependencies,
        RelationalQueryModelVisitorDependencies relationalDependencies,
        RelationalQueryCompilationContext compilationContext,
        RelationalQueryModelVisitor modelVisitor) :
        base(dependencies, relationalDependencies, compilationContext, modelVisitor)
    {
        var field = typeof(EntityQueryModelVisitor).GetField("_modelExpressionApplyingExpressionVisitor", BindingFlags.Instance | BindingFlags.NonPublic);
        var expressionVisitor = new CustomModelExpressionApplyingExpressionVisitor(
            compilationContext,
            dependencies.QueryModelGenerator,
            this);
        field.SetValue(this, expressionVisitor);
    }
}