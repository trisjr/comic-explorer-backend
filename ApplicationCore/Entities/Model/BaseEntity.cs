using ApplicationCore.Annotations;

namespace ApplicationCore.Entities.Model;

public class BaseEntity
{
    public virtual Guid Id { get; protected set; }

    [DateTime(true)]
    public DateTime CreatedAt { get; set; }

    [DateTime]
    public DateTime UpdatedAt { get; set; }
}
