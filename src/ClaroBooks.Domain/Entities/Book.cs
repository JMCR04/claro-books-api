namespace ClaroBooks.Domain.Entities;

public sealed class Book
{
    public int Id { get; set; }

    public string? Title { get; set; }

    public string? Description { get; set; }

    public int PageCount { get; set; }

    public string? Excerpt { get; set; }

    public DateTimeOffset PublishDate { get; set; }
}