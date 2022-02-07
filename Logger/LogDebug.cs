using System.Diagnostics;

namespace MemBot.Logger;

public class LogDebug : LogBase
{
  protected override void LogConcrete(string info)
    => Debug.WriteLine($"{info}{Environment.NewLine}");
}