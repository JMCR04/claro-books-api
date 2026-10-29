using System.ComponentModel.DataAnnotations;

namespace ClaroBooks.Api.Contracts.Books;

public sealed class SaveBookRequest
{
    [Range(1, int.MaxValue,
        ErrorMessage = "El ID debe ser mayor que cero.")]
    public int Id { get; set; }

    [Required(ErrorMessage = "El título es obligatorio.")]
    [StringLength(200,
        ErrorMessage = "El título no puede superar los 200 caracteres.")]
    public string Title { get; set; } = string.Empty;

    [Required(ErrorMessage = "La descripción es obligatoria.")]
    public string Description { get; set; } = string.Empty;

    [Range(1, int.MaxValue,
        ErrorMessage = "La cantidad de páginas debe ser mayor que cero.")]
    public int PageCount { get; set; }

    public string? Excerpt { get; set; }

    [Required(ErrorMessage = "La fecha de publicación es obligatoria.")]
    public DateTimeOffset? PublishDate { get; set; }
}