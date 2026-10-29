using ClaroBooks.Domain.Entities;

namespace ClaroBooks.Application.Interfaces;

public interface IAuthorRepository
{
    Task<IReadOnlyList<Author>> GetAllAsync(
        CancellationToken cancellationToken = default);

    Task<Author?> GetByIdAsync(
        int id,
        CancellationToken cancellationToken = default);

    Task<IReadOnlyList<Author>> GetByBookIdAsync(
        int bookId,
        CancellationToken cancellationToken = default);

    Task CreateAsync(
        Author author,
        CancellationToken cancellationToken = default);

    Task UpdateAsync(
        int id,
        Author author,
        CancellationToken cancellationToken = default);

    Task DeleteAsync(
        int id,
        CancellationToken cancellationToken = default);
}