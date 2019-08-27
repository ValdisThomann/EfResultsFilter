using System;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Threading;
using EfFilter;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Internal;
using Microsoft.EntityFrameworkCore.Query;
using Microsoft.EntityFrameworkCore.Query.ExpressionVisitors.Internal;
using Microsoft.EntityFrameworkCore.Query.Internal;
using Remotion.Linq.Clauses.Expressions;
using Remotion.Linq.Parsing.ExpressionVisitors;

public class CustomModelExpressionApplyingExpressionVisitor :
    ModelExpressionApplyingExpressionVisitor
{
    QueryCompilationContext compilationContext;
    IQueryModelGenerator generator;
    internal static AsyncLocal<Filters> filters = new AsyncLocal<Filters>();
    IParameterValues parameters;

    Filters instanceFilters;
    static MethodInfo whereMethod;
    static FieldInfo parameterField;

    static CustomModelExpressionApplyingExpressionVisitor()
    {
        var whereMethodField = typeof(ModelExpressionApplyingExpressionVisitor).GetField("_whereMethod", BindingFlags.Static | BindingFlags.NonPublic);
        whereMethod = (MethodInfo) whereMethodField.GetValue(null);
        parameterField = typeof(ModelExpressionApplyingExpressionVisitor).GetField("_parameters", BindingFlags.Instance | BindingFlags.NonPublic);
    }

    public CustomModelExpressionApplyingExpressionVisitor(
        QueryCompilationContext compilationContext,
        IQueryModelGenerator generator,
        EntityQueryModelVisitor visitor) :
        base(compilationContext, generator, visitor)
    {
        this.compilationContext = compilationContext;
        this.generator = generator;
        instanceFilters = filters.Value;
        parameters = (IParameterValues) parameterField.GetValue(this);
    }

    protected override Expression VisitConstant(ConstantExpression constantExpression)
    {
        var expression = base.VisitConstant(constantExpression);

        if (!constantExpression.IsEntityQueryable())
        {
            return expression;
        }
        var type = ((IQueryable)constantExpression.Value).ElementType;
        var entityType = compilationContext.Model.FindEntityType(type)?.RootType();

        if (entityType == null)
        {
            return expression;
        }

        if(instanceFilters == null)
        {
            return expression;
        }

        var lambdaMethod = GetType().GetMethod("BuildLambda");
        lambdaMethod = lambdaMethod.MakeGenericMethod(type);
        var lambda = (LambdaExpression)lambdaMethod.Invoke(this,null);
        var parameterizedFilter
            = (LambdaExpression)generator
                .ExtractParameters(
                    compilationContext.Logger,
                    lambda,
                    parameters,
                    parameterize: false,
                    generateContextAccessors: true);

        var oldParameterExpression = parameterizedFilter.Parameters[0];
        var newParameterExpression = Expression.Parameter(type, oldParameterExpression.Name);

        var predicateExpression
            = ReplacingExpressionVisitor
                .Replace(
                    oldParameterExpression,
                    newParameterExpression,
                    parameterizedFilter.Body);

        var whereExpression
            = Expression.Call(
                whereMethod.MakeGenericMethod(type),
                expression,
                Expression.Lambda(
                    predicateExpression,
                    newParameterExpression));

        var subQueryModel = generator.ParseQuery(whereExpression);

        return new SubQueryExpression(subQueryModel);
    }

    public LambdaExpression BuildLambda<T>()
    {
        Expression<Func<T, bool>> shouldInclude = x => instanceFilters.ShouldInclude(x).GetAwaiter().GetResult();
        return shouldInclude;
    }
}