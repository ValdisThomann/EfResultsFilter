using System;

public class ChildEntity
{
    public Guid Id { get; set; } = XunitLogging.Context.NextGuid();
    public string Property { get; set; }
    public ParentEntity Parent { get; set; }
}