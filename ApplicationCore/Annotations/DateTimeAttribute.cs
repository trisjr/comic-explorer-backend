namespace ApplicationCore.Annotations;

[AttributeUsage(AttributeTargets.Property)]
public class DateTimeAttribute : Attribute
{
    public bool IsCreatedAt { get; }
    
    public DateTimeAttribute(bool isCreatedAt = false)
    {
        IsCreatedAt = isCreatedAt;
    }
}
