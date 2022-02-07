using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;

namespace MemBot
{
  public class Storage : StorageBase
  {
    protected override IMemMatching MemMatching { get; init; }
    private readonly DbContextOptions<EFApplicationContext> _options;
    public Storage(IMemMatching memMatching, string connection = "DefaultConnection")
    {
      var builder = new ConfigurationBuilder();
      builder.SetBasePath(Directory.GetCurrentDirectory());
      builder.AddJsonFile("appsettings.json");
      var config = builder.Build();

      string connectionString = config.GetConnectionString(connection);

      var optionsBuilder = new DbContextOptionsBuilder<EFApplicationContext>();
      _options = optionsBuilder.UseSqlite(connectionString).Options;
      MemMatching = memMatching;
    }

    public async override Task<bool> Add(Mem mem)
    {
      using EFApplicationContext db = new(_options);
      try
      {
        string tagsString = string.Concat(mem.Tags.Select(t => t.Name));

        var existedMem = await db.Mems.FirstOrDefaultAsync(m => mem.Text == m.Text);
        var oldTags = await db.Tags.Where(t => tagsString.Contains(t.Name)).ToListAsync();
        var newTags = mem.Tags.Except(oldTags, new MemTagComparer()).ToList();

        if (existedMem is not null)
          await UpdateExistedMem(db, existedMem, mem, newTags);
        else
          await UpdateOrAddMemMedia(db, mem).ContinueWith(async (task) => await db.Mems.AddAsync(mem));

        await db.Tags.AddRangeAsync(newTags);
        await db.SaveChangesAsync();
        return true;
      } catch
      {
        return false;
      }
    }

    public async override Task<List<Mem>> GetMatchedMems(string tagsString)
    {
      if (!Tag.HasTags(tagsString)) return new List<Mem>();
      return await GetMatchedMems(tagsString.Split(Tag.Separator, StringSplitOptions.RemoveEmptyEntries).ToList());
    }

    public async override Task<List<Mem>> GetMatchedMems(List<string> tags)
    {
      if (!tags.Any()) return new List<Mem>();
      using EFApplicationContext db = new(_options);
      try
      {
        IQueryable<Mem> memsDb = db.Mems.Include(m => m.Tags).Include(m => m.Media);
        return await MemMatching.GetByTagsFromDB(memsDb, tags);
      } catch
      {
        return new List<Mem>();
      }

    }

    public async override Task<List<Tag>> GetTags()
    {
      using EFApplicationContext db = new(_options);
      try
      {
        return await db.Tags.ToListAsync();
      } catch
      {
        return new List<Tag>();
      }
    }

    private static async Task UpdateExistedMem(EFApplicationContext db, Mem existedMem, Mem mem, List<Tag> newTags)
    {
      mem.Tags = mem.Tags.Except(existedMem.Tags).ToList();
      existedMem.Tags.AddRange(newTags);
      if (existedMem.Media is null || existedMem.Media.Count == 0)
      {
        existedMem.Media = new() { mem.Media.First() };
        await UpdateOrAddMemMedia(db, existedMem);
      }
      db.Update(existedMem);
    }

    private static async Task UpdateOrAddMemMedia(EFApplicationContext db, Mem mem)
    {
      if (mem.Media is null || mem.Media.Count != 1) return;
      var (isExist, fileName) = mem.Media.First().IsAlreadyExist();
      Media foundMedia;
      if (!isExist ||
         (foundMedia = db.Media.FirstOrDefault(m => m.Name == fileName)!) is null)
      {
        await mem.Media.First().Save();
        await db.Media.AddRangeAsync(mem.Media);
      } else mem.Media.Add(foundMedia);
    }
  }
}