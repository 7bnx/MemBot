namespace MemBot
{
  public class StorageProxy : StorageBase
  {
    private readonly object _locker = new();
    private readonly Storage _storage;
    private readonly Dictionary<string, List<Mem>> _memsCache = new();
    private List<Tag> _tagsCache = new();
    protected override IMemMatching MemMatching { get; init; }

    public StorageProxy(IMemMatching memMatching, string connectionString = "DefaultConnection")
    {
      MemMatching = memMatching;
      _storage = new(memMatching, connectionString);
    }
    public async override Task<bool> Add(Mem mem)
    {
      var isAdded = await _storage.Add(mem);
      if (isAdded)
      {
        AddTagsToCache(mem.Tags);
        AddNewMemToCache(mem);
      } 
      return isAdded;
    }
    public async override Task<List<Mem>> GetMatchedMems(List<string> tags)
    {
      bool isContainsAll = TryGetAllMemsFromCache(tags, out var tagsNotContain, out var list);

      if (!isContainsAll)
        list.AddRange(await GetMemsFromStorage(tagsNotContain));

      return list.Distinct().ToList();
    }
    public async override Task<List<Mem>> GetMatchedMems(string tagsString)
    {
      var tags = Tag.ToTagArray(tagsString).ToList();
      return await GetMatchedMems(tags);
    }

    public async override Task<List<Tag>> GetTags()
    {
      if (_tagsCache.Count == 0)
        _tagsCache.AddRange(await _storage.GetTags());
      return _tagsCache;
    }

    private void AddNewMemToCache(Mem mem)
    {
      lock (_locker)
      {
        foreach (var key in _memsCache.Keys)
        {
          foreach (var tag in mem.Tags)
          {
            if (key.Contains(tag.Name) || tag.Name.Contains(key))
            {
              _memsCache[key].Remove(mem);
              _memsCache[key].Add(mem);
              break;
            }
          }
        }
      }
    }

    private bool TryGetAllMemsFromCache(List<string> tags, out List<string> tagsNotContain, 
                                        out List<Mem> list)
    {
      bool isContainsAll = false;
      tagsNotContain = new(tags);
      list = new();
      lock (_locker)
      {
        foreach(var tag in tags)
        {
          foreach (var key in _memsCache.Keys)
          {
            if (key == tag)
            {
              list.AddRange(_memsCache[key]);
              tagsNotContain.Remove(tag);
            }
          }
        }
      }
      return isContainsAll;
    }

    private async Task<List<Mem>> GetMemsFromStorage(List<string> tags)
    {
      var mems = await _storage.GetMatchedMems(tags);
      var list = new List<Mem>();
      foreach (var tag in tags)
      {
        List<Mem> memsByTag = new();
        foreach (var mem in mems)
          if (mem.Tags.Any(t => t.Name.Contains(tag))) memsByTag.Add(mem);
        if (memsByTag.Count != 0)
        {
          _memsCache.Add(tag, memsByTag);
          list.AddRange(memsByTag);
        }
      }
      return list;
    }

    private void AddTagsToCache(List<Tag> tags)
    {
      if (_tagsCache.Count != 0)
      {
        _tagsCache.AddRange(tags);
        _tagsCache = _tagsCache.Distinct().ToList();
      }
    }
  }
}