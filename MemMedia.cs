using System.ComponentModel.DataAnnotations.Schema;
using System;

namespace MemBot
{
  abstract class MemMedia
  {
    private const string _directory = "Media";
    private const string _dateFormatDirectory = "yyyy_MM_dd";
    public int Id { get; set; }
    public string Name { get; set; } = null!;
    public string Extension { get; set; } = null!;
    public int Size { get; set; }
    //public int MemId { get; set; }
    //public Mem Mem { get; set; } = null!;
    public List<Mem> Mems { get; set; } = new();
    [NotMapped]
    public byte[] Data { get; set; } = null!;
    public abstract string Type { get; init; }
    
    public async Task<(bool, string)> IsExistAsync()
    {
      var directoryPath = Path.Combine(_directory, Type);
      if (!Directory.Exists(directoryPath)) return (false, string.Empty);
      var filesPaths = Directory.GetFiles(directoryPath, "*.*", SearchOption.AllDirectories);
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

    private async Task<(bool, string)> IsSameDataAsync(List<FileInfo> files)
    {
      foreach(var file in files)
      {
        using var fs = file.OpenRead();
        byte[] readByte = new byte[2];
        await fs.ReadAsync(readByte.AsMemory(0, 2));
        if (readByte[0] == Data[0] && readByte[1] == Data[1]) return (true, file.Name.Split('.').First());
      }
      return (false, string.Empty);
    }

    public async Task Save()
    {
      string pathToDirectory = Path.Combine(_directory, Type, DateTime.Now.ToString(_dateFormatDirectory));
      if(!Directory.Exists(pathToDirectory)) Directory.CreateDirectory(pathToDirectory);
      string fullPAth = Path.Combine(pathToDirectory, Name + Extension);
      await File.WriteAllBytesAsync(fullPAth, Data);
    }
    
  }
}
