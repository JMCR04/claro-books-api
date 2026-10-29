using ClaroBooks.Domain.Entities;

namespace ClaroBooks.Application.Interfaces;

public interface IBookRepository
{
    Task<IReadOnlyList<Book>> GetAllAsync(
        CancellationToken cancellationToken = default);

    Task<Book?> GetByIdAsync(
        int id,
        CancellationToken cancellationToken = default);

    Task CreateAsync(
        Book book,
        CancellationToken cancellationToken = default);

    Task UpdateAsync(
        int id,
        Book book,
        CancellationToken cancellationToken = default);

    Task DeleteAsync(
        int id,
        CancellationToken cancellationToken = default);
}