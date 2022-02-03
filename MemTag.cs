namespace MemBot
{

  internal class MemTag : IEquatable<MemTag>
  {
    public const string Separator = "#";
    public int Id { get; set; }
    public string Name { get; set; } = null!;
    public List<Mem> Mems { get; set; } = new();
    public MemTag(string tag = "") => Name = tag.Trim().ToLower();

    public MemTag() { }

    public static bool HasTags(string str) 
      => str?.IndexOf(Separator) == 0 && str.Length >= 2;
    
    public static string[] ToTagArray(string str)
      => str.ToLower()
            .Split(Separator, StringSplitOptions.RemoveEmptyEntries)
            .Select(s => string.Join("", s.Trim().Where(c => char.IsLetterOrDigit(c))))
            .Distinct().ToArray();
    
    public static MemTag[] ToArray(string str)
      => ToTagArray(str).Select(s => new MemTag(s)).ToArray();

    public bool Equals(MemTag? other)
      => other is not null && Name == other.Name.ToLower();

    public override string ToString()
      => Separator + Name;

    public override bool Equals(object? obj)
      => Equals(obj as MemTag);

    public override int GetHashCode()
      => Name.GetHashCode();
  }

  class MemTagComparer : IEqualityComparer<MemTag>
  {
    public bool Equals(MemTag? x, MemTag? y)
      => x is not null && y is not null && x!.Name == y!.Name;

    public int GetHashCode(MemTag obj)
      => obj.Name.GetHashCode();
  }

}
