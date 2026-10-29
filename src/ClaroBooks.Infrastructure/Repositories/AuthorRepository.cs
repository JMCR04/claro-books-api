using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using ClaroBooks.Application.Interfaces;
using ClaroBooks.Domain.Entities;
using ClaroBooks.Infrastructure.State;

namespace ClaroBooks.Infrastructure.Repositories;

public sealed class AuthorRepository : IAuthorRepository
{
    private readonly HttpClient _httpClient;
    private readonly TemporaryChanges<Author> _changes;

    public AuthorRepository(
        HttpClient httpClient,
        TemporaryChanges<Author> changes)
    {
        _httpClient = httpClient;
        _changes = changes;
    }

    public async Task<IReadOnlyList<Author>> GetAllAsync(
        CancellationToken cancellationToken = default)
    {
        using var response = await _httpClient.GetAsync(
            "Authors",
            cancellationToken);

        response.EnsureSuccessStatusCode();

        var authors = await response.Content.ReadFromJsonAsync<List<Author>>(
            cancellationToken: cancellationToken)
            ?? throw new JsonException(
                "La API externa devolvió una lista de autores nula.");

        return _changes.Merge(authors, author => author.Id);
    }

    public async Task<Author?> GetByIdAsync(
        int id,
        CancellationToken cancellationToken = default)
    {
        using var response = await _httpClient.GetAsync(
            $"Authors/{id}",
            cancellationToken);

        if (response.StatusCode == HttpStatusCode.NotFound)
        {
            return _changes.Get(id, null);
        }

        response.EnsureSuccessStatusCode();

        var author = await response.Content.ReadFromJsonAsync<Author>(
            cancellationToken: cancellationToken)
            ?? throw new JsonException(
                "La API externa devolvió un autor nulo.");

        return _changes.Get(id, author);
    }

    public async Task<IReadOnlyList<Author>> GetByBookIdAsync(
        int bookId,
        CancellationToken cancellationToken = default)
    {
        var authors = await GetAllAsync(cancellationToken);

        return authors
            .Where(author => author.IdBook == bookId)
            .ToArray();
    }

    public async Task CreateAsync(
        Author author,
        CancellationToken cancellationToken = default)
    {
        using var response = await _httpClient.PostAsJsonAsync(
            "Authors",
            author,
            cancellationToken);

        response.EnsureSuccessStatusCode();

        _changes.Save(author.Id, author);
    }

    public async Task UpdateAsync(
        int id,
        Author author,
        CancellationToken cancellationToken = default)
    {
        using var response = await _httpClient.PutAsJsonAsync(
            $"Authors/{id}",
            author,
            cancellationToken);

        response.EnsureSuccessStatusCode();

        _changes.Save(id, author);
    }

    public async Task DeleteAsync(
        int id,
        CancellationToken cancellationToken = default)
    {
        using var response = await _httpClient.DeleteAsync(
            $"Authors/{id}",
            cancellationToken);

        response.EnsureSuccessStatusCode();

        _changes.Remove(id);
    }
}