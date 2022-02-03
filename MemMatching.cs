using Microsoft.EntityFrameworkCore;
using System.Linq;
using LinqKit;
using System.Data.Entity.Infrastructure;

namespace MemBot
{
  static internal class MemMatching
  {
    public static async Task<List<Mem>> ByTagsCollectionFromDb(IQueryable<Mem> mems, IEnumerable<string> tags)
    {
      List<Mem> list = new();
      foreach (var tag in tags)
        list.AddRange(await mems.Where(m => m.Tags.Any(t => t.Name.Contains(tag))).ToListAsync());

      list = list.Distinct().ToList();
      return list;
    }

    public static async Task<List<Mem>> ByTagStringFromDb(IQueryable<Mem> mems, string tagsString)
      => await ByTagsCollectionFromDb(mems, MemTag.ToTagArray(tagsString));

    public static Mem GetRandomMem(IEnumerable<Mem> mems, string tagsString)
    {
      var tags = MemTag.ToTagArray(tagsString);
      var memsMatchCount = mems.Select(m => new 
                                      { Mem = m, Count = tags.Count(tag => m.Tags.Any(t => t.Name.Contains(tag))) })       //tagsString.Contains(t.Name)) })
                               .ToArray();
      if (memsMatchCount.Length == 0) return new();
      if (memsMatchCount.Length == 1) return memsMatchCount[0].Mem;
      var maxMatchCount = memsMatchCount.Max(m => m.Count);
      var maxMatchedMems = memsMatchCount.Where(m => m.Count == maxMatchCount);
      Random rand = new();
      return maxMatchedMems.ElementAt(rand.Next(0, maxMatchedMems.Count())).Mem;
    }
    public static Mem GetRandomMem(IEnumerable<Mem> mems, IEnumerable<string> tags)
      => GetRandomMem(mems, string.Concat(tags));

  }

}
