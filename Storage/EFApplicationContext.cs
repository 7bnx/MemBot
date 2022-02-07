using Microsoft.EntityFrameworkCore;

namespace MemBot
{
  public class EFApplicationContext : DbContext
  {
    public DbSet<Mem> Mems => Set<Mem>();
    public DbSet<Media> Media => Set<Media>();
    private DbSet<Video> Video => Set<Video>();
    private DbSet<Audio> Audio => Set<Audio>();
    private DbSet<Image> Image => Set<Image>();
    public DbSet<Tag> Tags => Set<Tag>();
    public EFApplicationContext(DbContextOptions<EFApplicationContext> options) : base(options) => Database.EnsureCreated();

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
      optionsBuilder.UseSqlite("Data Source=membot.db");
    }
  }
}
