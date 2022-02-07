namespace MemBot.Logger;

public class LogConsole : LogBase
{
  protected override void LogConcrete(string info) 
    => Console.WriteLine(info);
}