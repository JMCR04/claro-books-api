using System.ComponentModel.DataAnnotations;
using ClaroBooks.Api.Contracts.Books;
using ClaroBooks.Application.Interfaces;
using ClaroBooks.Domain.Entities;
using Microsoft.AspNetCore.Mvc;

namespace ClaroBooks.Api.Controllers;

[ApiController]
[Route("api/books")]
public sealed class BooksController : ControllerBase
{
    private readonly IBookRepository _bookRepository;
    private readonly IAuthorRepository _authorRepository;

    public BooksController(
        IBookRepository bookRepository,
        IAuthorRepository authorRepository)
    {
        _bookRepository = bookRepository;
        _authorRepository = authorRepository;
    }

    [HttpGet]
    public async Task<ActionResult<IReadOnlyList<Book>>> GetAll(
        CancellationToken cancellationToken)
    {
        var books = await _bookRepository.GetAllAsync(cancellationToken);

        return Ok(books);
    }

    [HttpGet("{id:int}")]
    public async Task<ActionResult<Book>> GetById(
        [Range(1, int.MaxValue)] int id,
        CancellationToken cancellationToken)
    {
        var book = await _bookRepository.GetByIdAsync(
            id,
            cancellationToken);

        if (book is null)
        {
            return Problem(
                statusCode: StatusCodes.Status404NotFound,
                title: "Libro no encontrado",
                detail: $"No existe un libro con el ID {id}.");
        }

        return Ok(book);
    }

    [HttpGet("{id:int}/authors")]
    public async Task<ActionResult<IReadOnlyList<Author>>> GetAuthors(
        [Range(1, int.MaxValue)] int id,
        CancellationToken cancellationToken)
    {
        var book = await _bookRepository.GetByIdAsync(
            id,
            cancellationToken);

        if (book is null)
        {
            return Problem(
                statusCode: StatusCodes.Status404NotFound,
                title: "Libro no encontrado",
                detail: $"No existe un libro con el ID {id}.");
        }

        var authors = await _authorRepository.GetByBookIdAsync(
            id,
            cancellationToken);

        return Ok(authors);
    }

    [HttpPost]
    public async Task<ActionResult<Book>> Create(
        [FromBody] SaveBookRequest request,
        CancellationToken cancellationToken)
    {
        var existingBook = await _bookRepository.GetByIdAsync(
            request.Id,
            cancellationToken);

        if (existingBook is not null)
        {
            return Problem(
                statusCode: StatusCodes.Status409Conflict,
                title: "ID de libro duplicado",
                detail: $"Ya existe un libro con el ID {request.Id}.");
        }

        var book = MapToBook(request);

        await _bookRepository.CreateAsync(
            book,
            cancellationToken);

        return Ok(book);
    }

    [HttpPut("{id:int}")]
    public async Task<ActionResult<Book>> Update(
        [Range(1, int.MaxValue)] int id,
        [FromBody] SaveBookRequest request,
        CancellationToken cancellationToken)
    {
        if (id != request.Id)
        {
            ModelState.AddModelError(
                nameof(request.Id),
                "El ID de la ruta debe coincidir con el ID del libro.");

            return ValidationProblem(ModelState);
        }

        var existingBook = await _bookRepository.GetByIdAsync(
            id,
            cancellationToken);

        if (existingBook is null)
        {
            return Problem(
                statusCode: StatusCodes.Status404NotFound,
                title: "Libro no encontrado",
                detail: $"No existe un libro con el ID {id}.");
        }

        var book = MapToBook(request);

        await _bookRepository.UpdateAsync(
            id,
            book,
            cancellationToken);

        return Ok(book);
    }

    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Delete(
        [Range(1, int.MaxValue)] int id,
        CancellationToken cancellationToken)
    {
        var existingBook = await _bookRepository.GetByIdAsync(
            id,
            cancellationToken);

        if (existingBook is null)
        {
            return Problem(
                statusCode: StatusCodes.Status404NotFound,
                title: "Libro no encontrado",
                detail: $"No existe un libro con el ID {id}.");
        }

        await _bookRepository.DeleteAsync(
            id,
            cancellationToken);

        return NoContent();
    }

    private static Book MapToBook(SaveBookRequest request)
    {
        return new Book
        {
            Id = request.Id,
            Title = request.Title.Trim(),
            Description = request.Description.Trim(),
            PageCount = request.PageCount,
            Excerpt = request.Excerpt?.Trim(),
            PublishDate = request.PublishDate!.Value
        };
    }
}