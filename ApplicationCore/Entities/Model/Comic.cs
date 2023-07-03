namespace ApplicationCore.Entities.Model;

public class Comic : BaseEntity
{
    public string Title { get; set; } = string.Empty;
    public string ImageUrl { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public Guid CategoryId { get; set; }
    public Guid AuthorId { get; set; }

    public ICollection<ComicAndCategory> ComicAndCategories { get; set; } =
        new List<ComicAndCategory>();
}
