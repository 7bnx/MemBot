using Telegram.Bot.Types.Enums;

namespace MemBot
{
  internal interface IMemMediaBuilder
  {
    MemMedia Create(string type);
    MemMedia Create(MessageType type, string extension = "");
  }
}
