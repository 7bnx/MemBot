using System.ComponentModel.DataAnnotations.Schema;

namespace MemBot
{
  public abstract class Media
  {
    public enum Types
    {
      Document,
      Audio,
      Video,
      Image
    };

    private const string _directory = "Media";
    private const string _dateFormatDirectory = "yyyy_MM_dd";
    private string DirectoryPath
      => System.IO.Path.Combine(_directory, Type.ToString(), CreationTime.ToString(_dateFormatDirectory));
    public int Id { get; set; }
    public string Name { get; set; } = null!;
    public string Extension { get; set; } = null!;
    public int Size { get; set; }
    public DateTime CreationTime { get; set; } = DateTime.Now;
    public abstract Types Type { get; init; }
    public List<Mem> Mems { get; set; } = new();
    [NotMapped]
    public byte[] Data { get; set; } = null!;
    public string Path => System.IO.Path.Combine(DirectoryPath, Name + Extension);

    public (bool isExist, string fileName) IsAlreadyExist()
    {
      var directoryTypePath = System.IO.Path.Combine(_directory, Type.ToString());
      if (!Directory.Exists(directoryTypePath)) return (false, string.Empty);
      var filesPaths = Directory.GetFiles(directoryTypePath, "*.*", SearchOption.AllDirectories);
      var list = SameSizeExtension(filesPaths);
      return IsSameData(list);
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

    private (bool isExist, string fileName) IsSameData(List<FileInfo> files)
    {
      bool isExist = false;
      string fileName = string.Empty;
      Parallel.ForEach(files, (file, state) => {
        byte[] readByte = new byte[2];
        using var fs = file.OpenRead();
        fs.Read(readByte, 0, 2);
        if (readByte[0] == Data[0] && readByte[1] == Data[1])
        {
          isExist = true;
          fileName = file.Name.Split('.').First();
          state.Stop();
        }
      });
      return (isExist, fileName);
    }

    public async Task Save()
    {
      try
      { 
        if (!Directory.Exists(DirectoryPath))
          Directory.CreateDirectory(DirectoryPath);
        await File.WriteAllBytesAsync(Path, Data);
      }catch
      {
        throw;
      }
    }
  }
}
