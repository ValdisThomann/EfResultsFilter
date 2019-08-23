using System;
using System.Collections.Generic;
using System.Diagnostics;

public class ParentEntity
{
    public ParentEntity()
    {
        Debug.WriteLine("sdf");
    }
    public Guid Id { get; set; } = XunitLogging.Context.NextGuid();
    public string Property { get; set; }
    public IList<ChildEntity> Children { get; set; } = new List<ChildEntity>();
}