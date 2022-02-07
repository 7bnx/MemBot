namespace MemBot
{
  public interface IStorage
  {
    Task<bool> Add(Mem mem);
    Task<List<Mem>> GetMatchedMems(string tags);
    Task<List<Mem>> GetMatchedMems(List<string> tags);
    Task<Mem> GetMem(string tags);
    Task<Mem> GetMem(List<string> tags);
    Task<List<Tag>> GetTags();
  }
}
