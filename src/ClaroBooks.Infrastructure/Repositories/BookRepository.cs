using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using ClaroBooks.Application.Interfaces;
using ClaroBooks.Domain.Entities;
using ClaroBooks.Infrastructure.State;

namespace ClaroBooks.Infrastructure.Repositories;

public sealed class BookRepository : IBookRepository
{
    private readonly HttpClient _httpClient;
    private readonly TemporaryChanges<Book> _changes;

    public BookRepository(
        HttpClient httpClient,
        TemporaryChanges<Book> changes)
    {
        _httpClient = httpClient;
        _changes = changes;
    }

    public async Task<IReadOnlyList<Book>> GetAllAsync(
        CancellationToken cancellationToken = default)
    {
        using var response = await _httpClient.GetAsync(
            "Books",
            cancellationToken);

        response.EnsureSuccessStatusCode();

        var books = await response.Content.ReadFromJsonAsync<List<Book>>(
            cancellationToken: cancellationToken)
            ?? throw new JsonException(
                "La API externa devolvió una lista de libros nula.");

        return _changes.Merge(books, book => book.Id);
    }

    public async Task<Book?> GetByIdAsync(
        int id,
        CancellationToken cancellationToken = default)
    {
        using var response = await _httpClient.GetAsync(
            $"Books/{id}",
            cancellationToken);

        if (response.StatusCode == HttpStatusCode.NotFound)
        {
            return _changes.Get(id, null);
        }

        response.EnsureSuccessStatusCode();

        var book = await response.Content.ReadFromJsonAsync<Book>(
            cancellationToken: cancellationToken)
            ?? throw new JsonException(
                "La API externa devolvió un libro nulo.");

        return _changes.Get(id, book);
    }

    public async Task CreateAsync(
        Book book,
        CancellationToken cancellationToken = default)
    {
        using var response = await _httpClient.PostAsJsonAsync(
            "Books",
            book,
            cancellationToken);

        response.EnsureSuccessStatusCode();

        _changes.Save(book.Id, book);
    }

    public async Task UpdateAsync(
        int id,
        Book book,
        CancellationToken cancellationToken = default)
    {
        using var response = await _httpClient.PutAsJsonAsync(
            $"Books/{id}",
            book,
            cancellationToken);

        response.EnsureSuccessStatusCode();

        _changes.Save(id, book);
    }

    public async Task DeleteAsync(
        int id,
        CancellationToken cancellationToken = default)
    {
        using var response = await _httpClient.DeleteAsync(
            $"Books/{id}",
            cancellationToken);

        response.EnsureSuccessStatusCode();

        _changes.Remove(id);
    }
}