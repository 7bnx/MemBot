using Microsoft.EntityFrameworkCore;

namespace MemBot
{
  public class MemMatching : IMemMatching
  {
    public async Task<List<Mem>> GetByTagsFromDB(IQueryable<Mem> mems, IEnumerable<string> tags)
    {
      List<Mem> list = new();
      foreach (var tag in tags)
        list.AddRange(await mems.Where(m => m.Tags.Any(t => t.Name.Contains(tag))).ToListAsync());

      list = list.Distinct().ToList();
      return list;
    }

    public async Task<List<Mem>> GetByTagsFromDB(IQueryable<Mem> mems, string tagsString)
      => await GetByTagsFromDB(mems, Tag.ToTagArray(tagsString));

    public Mem GetRandomMemByTags(IEnumerable<Mem> mems, string tagsString)
    {
      var count = mems.Count();
      if (count == 0) return null!;
      if (count == 1) return mems.ElementAt(0);
      var tags = Tag.ToTagArray(tagsString);
      var memsMatchCount = mems.Select(m => new 
                           { Mem = m, Count = tags.Count(tag => m.Tags.Any(t => t.Name.Contains(tag))) })
                           .ToArray();
      var maxMatchCount = memsMatchCount.Max(m => m.Count);
      var maxMatchedMems = memsMatchCount.Where(m => m.Count == maxMatchCount);
      Random rand = new();
      return maxMatchedMems.ElementAt(rand.Next(0, maxMatchedMems.Count())).Mem;
    }
    public Mem GetRandomMemByTags(IEnumerable<Mem> mems, IEnumerable<string> tags)
      => GetRandomMemByTags(mems, string.Concat(tags));

  }

}
