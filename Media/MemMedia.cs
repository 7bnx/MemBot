using System.ComponentModel.DataAnnotations.Schema;

namespace MemBot
{
  internal abstract class MemMedia
  {
    internal enum Types
    {
      Document,
      Audio,
      Video,
      Image
    };

    private const string _directory = "Media";
    private const string _dateFormatDirectory = "yyyy_MM_dd";
    private string DirectoryPath {
      get => Path.Combine(_directory, Type.ToString(), CreationTime.ToString(_dateFormatDirectory));
    }
    public int Id { get; set; }
    public string Name { get; set; } = null!;
    public string Extension { get; set; } = null!;
    public int Size { get; set; }
    public DateTime CreationTime { get; set; } = DateTime.Now;
    public abstract Types Type { get; init; }
    public List<Mem> Mems { get; set; } = new();
    [NotMapped]
    public byte[] Data { get; set; } = null!;
    public string GetPath() => Path.Combine(DirectoryPath, Name + Extension);

    public async Task<(bool isExist, string fileName)> IsAlreadyExistAsync()
    {
      var directoryTypePath = Path.Combine(_directory, Type.ToString());
      if (!Directory.Exists(directoryTypePath)) return (false, string.Empty);
      var filesPaths = Directory.GetFiles(directoryTypePath, "*.*", SearchOption.AllDirectories);
      var list = SameSizeExtension(filesPaths);
      return await IsSameDataAsync(list);
    }

    private List<FileInfo> SameSizeExtension(string[] filesPaths)
    {
      List<FileInfo> list = new();
      foreach (var filePath in filesPaths)
      {
        var file = new FileInfo(filePath);
        if (file.Length == Data.Length &&
            file.Extension == Extension) list.Add(file);
      }
      return list;
    }

    private async Task<(bool isExist, string fileName)> IsSameDataAsync(List<FileInfo> files)
    { 
      foreach(var file in files)
      {
        using var fs = file.OpenRead();
        byte[] readByte = new byte[2];
        await fs.ReadAsync(readByte.AsMemory(0, 2));
        if (readByte[0] == Data[0] && readByte[1] == Data[1]) 
          return (isExist: true, fileName: file.Name.Split('.').First());
      }
      return (isExist:false, fileName: string.Empty);
    }

    public void Save()
    {
      var task = Task.Run(() =>
      {
        if (!Directory.Exists(DirectoryPath)) 
          Directory.CreateDirectory(DirectoryPath);
        File.WriteAllBytes(GetPath(), Data);
      });
    }
  }
}
