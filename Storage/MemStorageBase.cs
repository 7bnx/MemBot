using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MemBot
{
  internal abstract class MemStorageBase : IMemStorage
  {
    public abstract Task<bool> Add(Mem mem);

    public abstract Task<IEnumerable<Mem>> GetAllMems(string tags);

    public abstract Task<IEnumerable<Mem>> GetAllMems(IEnumerable<string> tags);

    public async Task<Mem> GetMem(string tagsString)
    {
      var mems = await GetAllMems(tagsString);
      return MemMatching.GetRandomMem(mems, tagsString);
    }

    public async Task<Mem> GetMem(IEnumerable<string> tags)
    {
      var mems = await GetAllMems(tags);
      return MemMatching.GetRandomMem(mems, tags);
    }
  }
}
