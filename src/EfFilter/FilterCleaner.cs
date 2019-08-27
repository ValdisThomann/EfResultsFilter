using System;

class FilterCleaner:
    IDisposable
{
    public void Dispose()
    {
        ExpressionVisitor.filters.Value = null;
    }
}