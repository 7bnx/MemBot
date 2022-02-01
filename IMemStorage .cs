namespace MemBot
{
  internal interface IMemStorage
  {
    Task<bool> Add(Mem mem);
    Task<List<Mem>> Get(string tags);
    Task<List<Mem>> Get(IEnumerable<string> tags);
  }
}
