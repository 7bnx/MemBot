using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MemBot
{
  internal class MemStorageProxy : MemStorageBase
  {
    private readonly object _locker = new();
    private readonly MemStorage _storage;
    private readonly Dictionary<string, List<Mem>> _memsCache = new();

    public MemStorageProxy(string connectionString = "DefaultConnection") 
      => _storage = new(connectionString);
    public async override Task<bool> Add(Mem mem)
    {
      var isAdded = await _storage.Add(mem);
      if (isAdded)
      {
        lock (_locker)
        {
          foreach(var tag in mem.Tags)
            if (_memsCache.ContainsKey(tag.Name)) _memsCache.Remove(tag.Name);
        }
      }
      return isAdded;
    }
    public async override Task<IEnumerable<Mem>> GetAllMems(IEnumerable<string> a)
    {
      return new List<Mem>();
    }
    public async override Task<IEnumerable<Mem>> GetAllMems(string tagsString)
    {
      List<Mem> list = new();
      var tags = MemTag.ToArray(tagsString);
      bool isContainsAll = false;
      Dictionary<string, bool> containsDictionary;
      var tagsNotContain = new List<string>();
      lock (_locker)
      {
        containsDictionary = tags.ToDictionary(t => t.Name, t => _memsCache.ContainsKey(t.Name));
        isContainsAll = containsDictionary.All(t => t.Value);
        foreach (var tag in containsDictionary)
        {
          if (tag.Value) list.AddRange(_memsCache[tag.Key]);
          else tagsNotContain.Add(tag.Key);
        }
      }

      if (!isContainsAll)
      {
        var mems = await _storage.GetAllMems(tagsNotContain);
        foreach (var tag in containsDictionary)
        {
          if (tag.Value) continue;
          List<Mem> memsByTag = new();
          foreach (var mem in mems)
            if (mem.Tags.Any(t => t.Name.Contains(tag.Key))) memsByTag.Add(mem);
          if (memsByTag.Count != 0)
          {
            _memsCache.Add(tag.Key, memsByTag);
            list.AddRange(memsByTag);
          }
        }
      }
      return list.Distinct().ToList();
    }
  }
}