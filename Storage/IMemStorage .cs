namespace MemBot
{
  internal interface IMemStorage
  {
    Task<bool> Add(Mem mem);
    Task<IEnumerable<Mem>> GetAllMems(string tags);
    Task<IEnumerable<Mem>> GetAllMems(IEnumerable<string> tags);
    Task<Mem> GetMem(string tags);
    Task<Mem> GetMem(IEnumerable<string> tags);
  }
}
