using System;
namespace MemBot
{
  internal class MediaFactory : IMediaFactory
  {
    
    public MemMedia Create(MemMedia.Types type) => type switch
    {
      MemMedia.Types.Audio => new MemAudio(),
      MemMedia.Types.Image => new MemImage(),
      MemMedia.Types.Video => new MemVideo(),
      _ => new MemDocument()
    };

    public MemMedia Create(string extension)
    {
      MemMedia media = extension switch
      {
        ".jpg" or ".jpeg" or ".png" or ".gif" or ".bmp" or ".webp" => new MemImage(),
        "mp4" or "webm" or "m4v" => new MemVideo(),
        ".mp3" or ".wav" or "wma" => new MemAudio(),
        _ => new MemDocument()
      };
      media.Extension = extension;
      return media;
    }
  }
}
