namespace ApplicationCore.Entities.Model;

public class Category : BaseEntity
{
    public string CategoryName { get; set; } = string.Empty;

    public ICollection<ComicAndCategory> ComicAndCategories { get; set; } = null!;
}
