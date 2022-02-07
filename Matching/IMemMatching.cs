namespace MemBot
{
  public interface IMemMatching
  {
    Task<List<Mem>> GetByTagsFromDB(IQueryable<Mem> mems, IEnumerable<string> tags);
    Task<List<Mem>> GetByTagsFromDB(IQueryable<Mem> mems, string tagsString);
    Mem GetRandomMemByTags(IEnumerable<Mem> mems, IEnumerable<string> tags);
    Mem GetRandomMemByTags(IEnumerable<Mem> mems, string tagsString);
  }
}
