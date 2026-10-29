namespace ClaroBooks.Infrastructure.State;

public sealed class TemporaryChanges<T> where T : class
{
    private readonly object _sync = new();
    private readonly Dictionary<int, T> _changes = new();
    private readonly HashSet<int> _deletedIds = new();

    public IReadOnlyList<T> Merge(
        IEnumerable<T> externalItems,
        Func<T, int> getId)
    {
        var items = externalItems.ToDictionary(getId);

        lock (_sync)
        {
            foreach (var id in _deletedIds)
            {
                items.Remove(id);
            }

            foreach (var entry in _changes)
            {
                items[entry.Key] = entry.Value;
            }
        }

        return items.Values.OrderBy(getId).ToArray();
    }

    public T? Get(int id, T? externalItem)
    {
        lock (_sync)
        {
            if (_deletedIds.Contains(id))
            {
                return null;
            }

            return _changes.TryGetValue(id, out var item)
                ? item
                : externalItem;
        }
    }

    public void Save(int id, T item)
    {
        lock (_sync)
        {
            _changes[id] = item;
            _deletedIds.Remove(id);
        }
    }

    public void Remove(int id)
    {
        lock (_sync)
        {
            _changes.Remove(id);
            _deletedIds.Add(id);
        }
    }
}