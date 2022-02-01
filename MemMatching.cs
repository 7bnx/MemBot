using Microsoft.EntityFrameworkCore;

namespace MemBot
{
  static internal class MemMatching
  {
    public static async Task<List<Mem>> ByTagsCollectionFromDb(IQueryable<Mem> mems, IEnumerable<string> tags)
    {
      var tagsString = string.Concat(tags);
      return await ByTagStringFromDb(mems, tagsString);
    }


    public static async Task<List<Mem>> ByTagStringFromDb(IQueryable<Mem> mems, string tagsString)
    {
      if (tagsString.Length == 0) return new();
      return await mems.Where(m => m.Tags.Any(t => tagsString.Contains(t.Name)))
                       .ToListAsync();
    }
  }

}
