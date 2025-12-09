using System.Collections;
using System.Text.Json;

namespace GameLauncher;

/// <summary>
/// Generic repository for managing collections with constraints
/// </summary>
/// <typeparam name="T">Type that must be a class with parameterless constructor</typeparam>
public class GenericRepository<T> : IEnumerable<T> where T : class, new()
{
    private readonly List<T> _items = new List<T>();
    private readonly string _storageKey;
    
    public event EventHandler<ItemEventArgs<T>>? ItemAdded;
    public event EventHandler<ItemEventArgs<T>>? ItemRemoved;
    public event EventHandler? CollectionCleared;
    
    public GenericRepository(string storageKey)
    {
        _storageKey = storageKey;
    }
    
    public int Count => _items.Count;
    
    public void Add(T item)
    {
        if (item == null)
            throw new ArgumentNullException(nameof(item));
            
        _items.Add(item);
        OnItemAdded(item);
    }
    
    public bool Remove(T item)
    {
        bool removed = _items.Remove(item);
        if (removed)
            OnItemRemoved(item);
        return removed;
    }
    
    public void Clear()
    {
        _items.Clear();
        OnCollectionCleared();
    }
    
    public T? Find(Predicate<T> match)
    {
        return _items.Find(match);
    }
    
    public List<T> FindAll(Predicate<T> match)
    {
        return _items.FindAll(match);
    }
    
    public IEnumerator<T> GetEnumerator()
    {
        return _items.GetEnumerator();
    }
    
    IEnumerator IEnumerable.GetEnumerator()
    {
        return GetEnumerator();
    }
    
    // Iterator method using yield return
    public IEnumerable<T> GetItemsReverse()
    {
        for (int i = _items.Count - 1; i >= 0; i--)
        {
            yield return _items[i];
        }
    }
    
    // Iterator method with filtering
    public IEnumerable<T> GetItemsWhere(Predicate<T> predicate)
    {
        foreach (var item in _items)
        {
            if (predicate(item))
                yield return item;
        }
    }
    
    protected virtual void OnItemAdded(T item)
    {
        ItemAdded?.Invoke(this, new ItemEventArgs<T>(item));
    }
    
    protected virtual void OnItemRemoved(T item)
    {
        ItemRemoved?.Invoke(this, new ItemEventArgs<T>(item));
    }
    
    protected virtual void OnCollectionCleared()
    {
        CollectionCleared?.Invoke(this, EventArgs.Empty);
    }
}

/// <summary>
/// Event arguments for generic item events
/// </summary>
public class ItemEventArgs<T> : EventArgs
{
    public T Item { get; }
    
    public ItemEventArgs(T item)
    {
        Item = item;
    }
}

/// <summary>
/// Interface for cloneable game data
/// </summary>
public interface IGameData
{
    string GetDataKey();
    void Validate();
}

/// <summary>
/// Generic data manager with additional constraints
/// </summary>
public class DataManager<TData, TKey> 
    where TData : class, IGameData, new()
    where TKey : IComparable<TKey>
{
    private readonly Dictionary<TKey, TData> _dataStore = new Dictionary<TKey, TData>();
    
    public event EventHandler<DataEventArgs<TData, TKey>>? DataUpdated;
    
    public void Store(TKey key, TData data)
    {
        try
        {
            data.Validate();
            _dataStore[key] = data;
            OnDataUpdated(key, data);
        }
        catch (Exception ex)
        {
            throw new GameException($"Failed to store data for key {key}", ex);
        }
    }
    
    public TData? Retrieve(TKey key)
    {
        return _dataStore.TryGetValue(key, out var data) ? data : null;
    }
    
    public bool Remove(TKey key)
    {
        return _dataStore.Remove(key);
    }
    
    public IEnumerable<TData> GetAllData()
    {
        foreach (var kvp in _dataStore.OrderBy(x => x.Key))
        {
            yield return kvp.Value;
        }
    }
    
    protected virtual void OnDataUpdated(TKey key, TData data)
    {
        DataUpdated?.Invoke(this, new DataEventArgs<TData, TKey>(key, data));
    }
}

public class DataEventArgs<TData, TKey> : EventArgs
{
    public TKey Key { get; }
    public TData Data { get; }
    
    public DataEventArgs(TKey key, TData data)
    {
        Key = key;
        Data = data;
    }
}
