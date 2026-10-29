namespace ClaroBooks.Domain.Entities;

public sealed class Author
{
    public int Id { get; set; }

    public int IdBook { get; set; }

    public string? FirstName { get; set; }

    public string? LastName { get; set; }
}