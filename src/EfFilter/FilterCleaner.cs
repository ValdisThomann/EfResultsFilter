using System;

class FilterCleaner:
    IDisposable
{
    public void Dispose()
    {
        CustomModelExpressionApplyingExpressionVisitor.filters.Value = null;
    }
}