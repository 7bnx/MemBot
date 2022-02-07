namespace MemBot
{
  public class Mem : IEquatable<Mem>
  {
    public int Id { get; set; }
    public string Text { get; set; } = null!;
    public List<Tag> Tags { get; set; } = new();
    public List<Media> Media { get; set; } = new();
    public DateTime CreateTime { get; set; } = DateTime.Now;

    public bool Equals(Mem? other)
      => other is not null && Text.ToUpper() == other.Text.ToUpper();
    
    public override bool Equals(object? obj) 
      => Equals(obj as Mem);

    public static bool operator ==(Mem? mem1, Mem? mem2)
      => mem1 is not null && mem2 is not null && 
         mem1.Text.ToUpper() == mem2.Text.ToUpper();
    
    public static bool operator !=(Mem? mem1, Mem? mem2) 
      => !(mem1 == mem2);
    
    public override int GetHashCode() 
      => Text.GetHashCode();
  }
}
