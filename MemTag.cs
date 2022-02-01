
namespace MemBot
{

  internal class MemTag : IEquatable<MemTag>
  {
    public const string Separator = "#";
    public int Id { get; set; }
    public string Name { get; set; } = null!;
    public List<Mem> Mems { get; set; } = new();
    public MemTag(string tag = "") => Name = tag.ToLower();

    public MemTag() { }
    public static bool IsStringHasTags(string str) => str?.IndexOf(Separator) == 0;
    public static List<MemTag> ToList(string str)
    {
      return str.ToLower()
                .Split(Separator, StringSplitOptions.RemoveEmptyEntries)
                .Select(s => string.Join("", s.Trim()
                .Where(c => char.IsLetterOrDigit(c))))
                .Select(s => new MemTag(s))
                .Distinct().ToList();
    }
    public static bool HasTag(string str) => str.Contains(Separator);

    public bool Equals(MemTag? other)
    {
      if (other is null) return false;
      return Name == other.Name.ToLower();
    }
  }

  class MemTagComparer : IEqualityComparer<MemTag>
  {
    public bool Equals(MemTag? x, MemTag? y)
    {
      if (x is null || y is null) return false;
      return x.Name.ToLower() == y.Name;
    }

    public int GetHashCode(MemTag obj)
    {
      return obj.Name.GetHashCode();
    }
  }

}
