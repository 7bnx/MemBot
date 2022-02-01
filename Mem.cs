namespace MemBot
{
  internal class Mem : IEquatable<Mem>
  {
    public int Id { get; set; }
    public string Text { get; set; } = null!;
    public List<MemTag> Tags { get; set; } = new();
    public List<MemMedia> Media { get; set; } = new();
    public DateTime CreateTime { get; set; } = DateTime.Now;

    public bool Equals(Mem? other)
    {
      if (other is Mem mem)
      {
        return Text.ToUpper() == mem.Text.ToUpper();
      }
      return false;
    }
    public override bool Equals(object? obj) => Equals(obj as Mem);
    public static bool operator ==(Mem? mem1, Mem? mem2)
    {
      if (mem1 is null || mem2 is null) return false;
      return mem1.Text.ToUpper() == mem2.Text.ToUpper();
    }
    public static bool operator !=(Mem? mem1, Mem? mem2) => !(mem1 == mem2);
    public override int GetHashCode() => Text.GetHashCode();

  }

  class MemComparer : IEqualityComparer<Mem>
  {
    public bool Equals(Mem? x, Mem? y) => x == y;

    public int GetHashCode(Mem obj) => obj.GetHashCode();
  }
}
