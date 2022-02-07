using MemBot.Logger;

namespace MemBot
{
  public class ControllerBuilder
  {
    private readonly Controller _controller;
    public ControllerBuilder(Controller controller) => _controller = controller;

    public ControllerBuilder SetStorage(IStorage storage)
    {
      _controller.Storage = storage;
      return this;
    }

    public ControllerBuilder SetBot(IBot bot)
    {
      _controller.Bot = bot;
      return this;
    }

    public ControllerBuilder SetMediaFactory(IMediaFactory factory)
    {
      _controller.MediaFactory = factory;
      return this;
    }

    public ControllerBuilder SetLogger(ILog logger)
    {
      _controller.Logger = logger;
      return this;
    }
    public Controller Build() => _controller;
  }
}
