using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;

namespace MemBot
{
  internal class MemMediaBuilder : IMemMediaBuilder
  {
    public MemMedia Create(string type)
    {
      return type.ToUpper() switch
      {
        "AUDIO" => new MemAudio(),
        "IMAGE" => new MemImage(),
        "VIDEO" => new MemVideo(),
        _ => new MemDocument()
      };
    }

    public MemMedia Create(MessageType type, string extension = "")
    {
      MemMedia media = type switch
      {
        MessageType.Audio => new MemAudio(),
        MessageType.Photo => new MemImage(),
        MessageType.Video => new MemVideo(),
        _ => new MemDocument()
      };
      if (extension != string.Empty) media.Extension = extension;
      return media;
    }
  }
}
