using System;

class FilterCleaner:
    IDisposable
{
    public void Dispose()
    {
        CustomQueryBuffer.filters.Value = null;
    }
}