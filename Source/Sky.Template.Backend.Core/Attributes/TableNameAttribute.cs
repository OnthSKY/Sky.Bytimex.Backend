namespace Sky.Template.Backend.Core.Attributes;

[AttributeUsage(AttributeTargets.Class, Inherited = false)]
public class TableNameAttribute : Attribute
{
    public string Name { get; }

    public TableNameAttribute(string name)
    {
        Name = name;
    }
}