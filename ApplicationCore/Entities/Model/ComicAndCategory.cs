namespace ApplicationCore.Entities.Model;

public class ComicAndCategory : BaseEntity
{
    public Guid ComicId { get; set; }
    public Guid CategoryId { get; set; }

    public Comic Comic { get; set; } = null!;
    public Category Category { get; set; } = null!;
}
