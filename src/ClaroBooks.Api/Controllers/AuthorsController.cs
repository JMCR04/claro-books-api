using System.ComponentModel.DataAnnotations;
using ClaroBooks.Api.Contracts.Authors;
using ClaroBooks.Application.Interfaces;
using ClaroBooks.Domain.Entities;
using Microsoft.AspNetCore.Mvc;

namespace ClaroBooks.Api.Controllers;

[ApiController]
[Route("api/authors")]
public sealed class AuthorsController : ControllerBase
{
    private readonly IAuthorRepository _authorRepository;
    private readonly IBookRepository _bookRepository;

    public AuthorsController(
        IAuthorRepository authorRepository,
        IBookRepository bookRepository)
    {
        _authorRepository = authorRepository;
        _bookRepository = bookRepository;
    }

    [HttpGet]
    public async Task<ActionResult<IReadOnlyList<Author>>> GetAll(
        CancellationToken cancellationToken)
    {
        var authors = await _authorRepository.GetAllAsync(
            cancellationToken);

        return Ok(authors);
    }

    [HttpGet("{id:int}")]
    public async Task<ActionResult<Author>> GetById(
        [Range(1, int.MaxValue)] int id,
        CancellationToken cancellationToken)
    {
        var author = await _authorRepository.GetByIdAsync(
            id,
            cancellationToken);

        if (author is null)
        {
            return Problem(
                statusCode: StatusCodes.Status404NotFound,
                title: "Autor no encontrado",
                detail: $"No existe un autor con el ID {id}.");
        }

        return Ok(author);
    }

    [HttpPost]
    public async Task<ActionResult<Author>> Create(
        [FromBody] SaveAuthorRequest request,
        CancellationToken cancellationToken)
    {
        var existingAuthor = await _authorRepository.GetByIdAsync(
            request.Id,
            cancellationToken);

        if (existingAuthor is not null)
        {
            return Problem(
                statusCode: StatusCodes.Status409Conflict,
                title: "ID de autor duplicado",
                detail: $"Ya existe un autor con el ID {request.Id}.");
        }

        var book = await _bookRepository.GetByIdAsync(
            request.IdBook,
            cancellationToken);

        if (book is null)
        {
            ModelState.AddModelError(
                nameof(request.IdBook),
                "El libro seleccionado no existe.");

            return ValidationProblem(ModelState);
        }

        var author = MapToAuthor(request);

        await _authorRepository.CreateAsync(
            author,
            cancellationToken);

        return Ok(author);
    }

    [HttpPut("{id:int}")]
    public async Task<ActionResult<Author>> Update(
        [Range(1, int.MaxValue)] int id,
        [FromBody] SaveAuthorRequest request,
        CancellationToken cancellationToken)
    {
        if (id != request.Id)
        {
            ModelState.AddModelError(
                nameof(request.Id),
                "El ID de la ruta debe coincidir con el ID del autor.");

            return ValidationProblem(ModelState);
        }

        var existingAuthor = await _authorRepository.GetByIdAsync(
            id,
            cancellationToken);

        if (existingAuthor is null)
        {
            return Problem(
                statusCode: StatusCodes.Status404NotFound,
                title: "Autor no encontrado",
                detail: $"No existe un autor con el ID {id}.");
        }

        var book = await _bookRepository.GetByIdAsync(
            request.IdBook,
            cancellationToken);

        if (book is null)
        {
            ModelState.AddModelError(
                nameof(request.IdBook),
                "El libro seleccionado no existe.");

            return ValidationProblem(ModelState);
        }

        var author = MapToAuthor(request);

        await _authorRepository.UpdateAsync(
            id,
            author,
            cancellationToken);

        return Ok(author);
    }

    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Delete(
        [Range(1, int.MaxValue)] int id,
        CancellationToken cancellationToken)
    {
        var existingAuthor = await _authorRepository.GetByIdAsync(
            id,
            cancellationToken);

        if (existingAuthor is null)
        {
            return Problem(
                statusCode: StatusCodes.Status404NotFound,
                title: "Autor no encontrado",
                detail: $"No existe un autor con el ID {id}.");
        }

        await _authorRepository.DeleteAsync(
            id,
            cancellationToken);

        return NoContent();
    }

    private static Author MapToAuthor(SaveAuthorRequest request)
    {
        return new Author
        {
            Id = request.Id,
            IdBook = request.IdBook,
            FirstName = request.FirstName.Trim(),
            LastName = request.LastName.Trim()
        };
    }
}