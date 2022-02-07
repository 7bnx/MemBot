namespace MemBot;

public interface IBot
{
  void Run();
  Task Send(string chat, Mem mem);
  Task Send(string chat, string message);
  event Action<string, string> OnReceivedTextMessage;
  event Action<string, Media.Types, string, Task<MemoryStream>> OnReceivedMediaMessage;
}
