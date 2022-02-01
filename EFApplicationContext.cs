using Microsoft.EntityFrameworkCore;

namespace MemBot
{
  internal class EFApplicationContext : DbContext
  {
    public DbSet<Mem> Mems => Set<Mem>();
    public DbSet<MemMedia> Media => Set<MemMedia>();
    private DbSet<MemVideo> Video => Set<MemVideo>();
    private DbSet<MemAudio> Audio => Set<MemAudio>();
    private DbSet<MemImage> Image => Set<MemImage>();
    public DbSet<MemTag> Tags => Set<MemTag>();
    public EFApplicationContext(DbContextOptions<EFApplicationContext> options) : base(options) => Database.EnsureCreated();

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
      optionsBuilder.UseSqlite("Data Source=membot.db");
    }
  }
}
