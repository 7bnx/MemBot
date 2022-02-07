using System;
namespace MemBot
{
  public class MediaFactory : IMediaFactory
  {
    
    public Media Create(Media.Types type) => type switch
    {
      Media.Types.Audio => new Audio(),
      Media.Types.Image => new Image(),
      Media.Types.Video => new Video(),
      _ => new Document()
    };

    public Media Create(string extension)
    {
      Media media = extension switch
      {
        ".jpg" or ".jpeg" or ".png" or ".gif" or ".bmp" or ".webp" => new Image(),
        "mp4" or "webm" or "m4v" => new Video(),
        ".mp3" or ".wav" or "wma" => new Audio(),
        _ => new Document()
      };
      media.Extension = extension;
      return media;
    }
  }
}
