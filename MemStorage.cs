using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;

namespace MemBot
{
  internal class MemStorage : IMemStorage
  {
    private readonly DbContextOptions<EFApplicationContext> _options;
    public MemStorage(string connection = "DefaultConnection")
    {
      var builder = new ConfigurationBuilder();
      builder.SetBasePath(Directory.GetCurrentDirectory());
      builder.AddJsonFile("appsettings.json");
      var config = builder.Build();

      string connectionString = config.GetConnectionString(connection);

      var optionsBuilder = new DbContextOptionsBuilder<EFApplicationContext>();
      _options = optionsBuilder.UseSqlite(connectionString).Options;
    }
    public async Task<bool> Add(Mem mem)
    {
      using EFApplicationContext db = new(_options);
      string tagsString = string.Concat(mem.Tags.Select(t => t.Name));

      var newTags = mem.Tags.Except(db.Tags, new MemTagComparer()).ToArray();
      var foundMem = await db.Mems.FirstOrDefaultAsync(m => mem.Text.ToUpper() == m.Text.ToUpper());

      if (foundMem is not null)
      {
        mem.Tags = mem.Tags.Except(foundMem.Tags).ToList();
        foundMem.Tags.AddRange(newTags);
        db.Update(foundMem);
      }
      else
      {
        var oldTags = await db.Tags.Where(t => tagsString.Contains(t.Name)).ToArrayAsync();
        //var oldTags = db.Tags.ToArray().Intersect(mem.Tags, new MemTagComparer()).ToArray();
        mem.Tags.Clear();
        mem.Tags.AddRange(newTags);
        mem.Tags.AddRange(oldTags);
        if (mem.Media.Count == 1)
        {
          var sameFileExist = await mem.Media.First().IsExistAsync();
          MemMedia foundMedia;
          if (!sameFileExist.Item1 ||
             (foundMedia = db.Media.ToArray().FirstOrDefault(m => m.Name == sameFileExist.Item2)!) is null)
          {
            await mem.Media.First().Save();
            await db.Media.AddRangeAsync(mem.Media);
          }
          else mem.Media = new() { foundMedia };
        }
        await db.Mems.AddAsync(mem);
      }

      await db.Tags.AddRangeAsync(newTags);

      await db.SaveChangesAsync();

      return true;
    }

    public async Task<List<Mem>> Get(string tagsString)
    {
      if (tagsString.Length < 2 || !MemTag.IsStringHasTags(tagsString)) return new();
      return await Get(tagsString.Split(MemTag.Separator, StringSplitOptions.RemoveEmptyEntries));
    }

    public async Task<List<Mem>> Get(IEnumerable<string> tags)
    {
      if (!tags.Any()) return new();
      using EFApplicationContext db = new(_options);
      
      IQueryable<Mem> memsDb = db.Mems.Include(m => m.Tags).Include(m => m.Media);
      return await MemMatching.ByTagsCollectionFromDb(memsDb, tags);

      //return ( db.Mems.Include(m => m.Tags).Include(m => m.Media))
                // .Where(m => tags.Any(t => m.Tags.Contains(t)))
                // .ToList();
      /*
      var r = mems.Select(m => new { Mem = m, Count = m.Tags.Count(t => tags.Contains(t)) })
                  .OrderByDescending(m => m.Count)
                  .ToArray();
      */
    }
  }
}
