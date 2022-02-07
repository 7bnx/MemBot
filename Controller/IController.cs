using MemBot.Logger;

namespace MemBot
{
  public interface IController
  {
    IStorage Storage { get; set; }
    IBot Bot { get; set; }
    IMediaFactory MediaFactory { get; set; }
    ILog Logger { get; set; }
    void Start();
  }
}
