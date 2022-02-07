namespace MemBot.Logger;
public abstract class LogBase : ILog
{
  public void Log(string info)
    => LogConcrete($"Time: {DateTime.Now}; Info: {info}");
  protected abstract void LogConcrete(string info);
}