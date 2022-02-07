namespace MemBot
{
  public abstract class StorageBase : IStorage
  {
    protected abstract IMemMatching MemMatching { get; init; }
    public abstract Task<bool> Add(Mem mem);

    public abstract Task<List<Mem>> GetMatchedMems(string tags);

    public abstract Task<List<Mem>> GetMatchedMems(List<string> tags);

    public async Task<Mem> GetMem(string tagsString)
    {
      var mems = await GetMatchedMems(tagsString);
      return MemMatching.GetRandomMemByTags(mems, tagsString);
    }

    public async Task<Mem> GetMem(List<string> tags)
    {
      var mems = await GetMatchedMems(tags);
      return MemMatching.GetRandomMemByTags(mems, tags);
    }

    public abstract Task<List<Tag>> GetTags();
  }
}
