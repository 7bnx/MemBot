using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using LinqKit;

namespace MemBot
{
  internal class MemStorage : MemStorageBase
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
    public async override Task<bool> Add(Mem mem)
    {
      using EFApplicationContext db = new(_options);
      string tagsString = string.Concat(mem.Tags.Select(t => t.Name));


      var foundMem = await db.Mems.FirstOrDefaultAsync(m => mem.Text == m.Text);
      var oldTags = await db.Tags.Where(t => tagsString.Contains(t.Name)).ToArrayAsync();
      var newTags = mem.Tags.Except(oldTags, new MemTagComparer()).ToArray();

      if (foundMem is not null)
      {
        mem.Tags = mem.Tags.Except(foundMem.Tags).ToList();
        foundMem.Tags.AddRange(newTags);
        db.Update(foundMem);
      }
      else
      {
        mem.Tags.Clear();
        mem.Tags.AddRange(newTags);
        mem.Tags.AddRange(oldTags);
        if (mem.Media.Count == 1)
        {
          var (isExist, fileName) = await mem.Media.First().IsAlreadyExistAsync();
          
          MemMedia foundMedia;
          if (!isExist ||
             (foundMedia = db.Media.ToArray().FirstOrDefault(m => m.Name == fileName)!) is null)
          {
            mem.Media.First().Save();
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

    public async override Task<IEnumerable<Mem>> GetAllMems(string tagsString)
    {
      if (!MemTag.HasTags(tagsString)) return new List<Mem>();
      return await GetAllMems(tagsString.Split(MemTag.Separator, StringSplitOptions.RemoveEmptyEntries));
    }

    public async override Task<IEnumerable<Mem>> GetAllMems(IEnumerable<string> tags)
    {
      if (!tags.Any()) return new List<Mem>();
      using EFApplicationContext db = new(_options);
      
      IQueryable<Mem> memsDb = db.Mems.Include(m => m.Tags).Include(m => m.Media);
      return  await MemMatching.ByTagsCollectionFromDb(memsDb, tags);

      /*
      var r = mems.Select(m => new { Mem = m, Count = m.Tags.Count(t => tags.Contains(t)) })
                  .OrderByDescending(m => m.Count)
                  .ToArray();
      */
    }
  }



}
