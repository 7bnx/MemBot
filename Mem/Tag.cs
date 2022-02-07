namespace MemBot
{

  public class Tag : IEquatable<Tag>
  {
    public const string Separator = "#";
    public int Id { get; set; }
    public string Name { get; set; } = null!;
    public List<Mem> Mems { get; set; } = new();
    public Tag(string tag = "") => Name = tag.Trim().ToLower();

    public Tag() { }

    public static bool HasTags(string str) 
      => str?.IndexOf(Separator) == 0 && str.Length >= 2;
    
    public static string[] ToTagArray(string str)
      => str.ToLower()
            .Split(Separator, StringSplitOptions.RemoveEmptyEntries)
            .Select(s => string.Join("", s.Trim().Where(c => char.IsLetterOrDigit(c))))
            .Distinct().ToArray();
    
    public static Tag[] ToArray(string str)
      => ToTagArray(str).Select(s => new Tag(s)).ToArray();

    public bool Equals(Tag? other)
      => other is not null && Name == other.Name.ToLower();

    public override string ToString()
      => Separator + Name;

    public override bool Equals(object? obj)
      => Equals(obj as Tag);

    public override int GetHashCode()
      => Name.GetHashCode();
  }

  class MemTagComparer : IEqualityComparer<Tag>
  {
    public bool Equals(Tag? x, Tag? y)
      => x is not null && y is not null && x!.Name == y!.Name;

    public int GetHashCode(Tag obj)
      => obj.Name.GetHashCode();
  }

}
