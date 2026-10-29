using System.ComponentModel.DataAnnotations;

namespace ClaroBooks.Api.Contracts.Authors;

public sealed class SaveAuthorRequest
{
    [Range(1, int.MaxValue,
        ErrorMessage = "El ID del autor debe ser mayor que cero.")]
    public int Id { get; set; }

    [Range(1, int.MaxValue,
        ErrorMessage = "El ID del libro debe ser mayor que cero.")]
    public int IdBook { get; set; }

    [Required(ErrorMessage = "El nombre es obligatorio.")]
    [StringLength(100,
        ErrorMessage = "El nombre no puede superar los 100 caracteres.")]
    public string FirstName { get; set; } = string.Empty;

    [Required(ErrorMessage = "El apellido es obligatorio.")]
    [StringLength(100,
        ErrorMessage = "El apellido no puede superar los 100 caracteres.")]
    public string LastName { get; set; } = string.Empty;
}