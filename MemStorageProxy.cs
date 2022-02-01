using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MemBot
{
  internal class MemStorageProxy : IMemStorage
  {
    private readonly object _locker = new();
    private readonly MemStorage _storage;
    private readonly Dictionary<string, List<Mem>> memsCache = new();

    public MemStorageProxy(string connection = "DefaultConnection") => _storage = new(connection);
    public async Task<bool> Add(Mem mem)
    {
      var isAdded = await _storage.Add(mem);
      if (isAdded)
      {
        var tags = mem.Tags.Select(t => t.Name).ToArray();
        lock (_locker)
        {
          foreach(var tag in tags)
          if (memsCache.ContainsKey(tag)) memsCache.Remove(tag);
        }
      }
      return isAdded;
    }
    public async Task<List<Mem>> Get(IEnumerable<string> a)
    {
      return new();
    }
    public async Task<List<Mem>> Get(string tags)
    {
      List<Mem> list = new();
      var tagsArr = tags.Split(MemTag.Separator);
      bool isContainAll = false;
      Dictionary<string, bool> containsDictionary;
      string tagsNotContain = string.Empty;
      lock (_locker)
      {
        containsDictionary = tagsArr.ToDictionary(t => t, t => memsCache.ContainsKey(t));
        isContainAll = containsDictionary.All(t => t.Value);
        foreach (var tag in containsDictionary)
        {
          if (tag.Value) list.AddRange(memsCache[tag.Key]);
          else tagsNotContain += MemTag.Separator + tag.Key;
        }
      }
      if (isContainAll) return list.Distinct().ToList();
      var mems = await _storage.Get(tagsNotContain);
      foreach (var tag in containsDictionary)
      {
        List<Mem> memsByTag = new();
        foreach (var mem in mems)
          if (mem.Tags.Contains(new MemTag(tag.Key))) memsByTag.Add(mem);
        if (memsByTag.Count != 0)
        {
          memsCache.Add(tag.Key, memsByTag);
          list.AddRange(memsByTag);
        }
      }

      return list.Distinct().ToList();
      /*
      string tagsOrdered = string.Join(MemTag.Separator, tags.Split(MemTag.Separator));
      if (!memsCache.ContainsKey(tagsOrdered))
      {
        var mem = await _storage.Get(tags);
        memsCache.Add(tagsOrdered, mem);
        list.AddRange(mem);
      }
      else
      {
        lock (_locker) list.AddRange(memsCache[tagsOrdered]);
      }
      return list;
      */
    }

    private static string GetOrderedTags(Mem mem) => 
      string.Join(MemTag.Separator, mem.Tags.OrderBy(t => t.Name));

  }
}
