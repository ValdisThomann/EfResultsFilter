using System;
using System.Collections.Concurrent;
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
    static ConcurrentDictionary<Type, MethodInfo> lambdaMethodCache = new ConcurrentDictionary<Type, MethodInfo>();
    IParameterValues parameters;

    Filters instanceFilters;
    static MethodInfo whereMethod;
    static FieldInfo parameterField;

    static CustomModelExpressionApplyingExpressionVisitor()
    {
        var modelVisitorType = typeof(ModelExpressionApplyingExpressionVisitor);
        var whereMethodField = modelVisitorType.GetField("_whereMethod", BindingFlags.Static | BindingFlags.NonPublic);
        whereMethod = (MethodInfo) whereMethodField.GetValue(null);
        parameterField = modelVisitorType.GetField("_parameters", BindingFlags.Instance | BindingFlags.NonPublic);
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

        var type = ((IQueryable) constantExpression.Value).ElementType;
        var entityType = compilationContext.Model.FindEntityType(type)?.RootType();

        if (entityType == null)
        {
            return expression;
        }

        if (instanceFilters == null)
        {
            return expression;
        }

        var lambda = BuildLambda(type);
        var parameterizedFilter
            = (LambdaExpression) generator
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

    LambdaExpression BuildLambda(Type type)
    {
        var lambdaMethod = GetLambdaMethod(type);
        return (LambdaExpression) lambdaMethod.Invoke(this, null);
    }

    static MethodInfo GetLambdaMethod(Type type)
    {
        return lambdaMethodCache.GetOrAdd(type, type1 =>
        {
            var method = typeof(CustomModelExpressionApplyingExpressionVisitor).GetMethod("BuildLambda");
            return method.MakeGenericMethod(type);
        });
    }

    public LambdaExpression BuildLambda<T>()
    {
        Expression<Func<T, bool>> shouldInclude = x => instanceFilters.ShouldInclude(x).GetAwaiter().GetResult();
        return shouldInclude;
    }
}