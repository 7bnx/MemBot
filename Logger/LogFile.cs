using System.Text;

namespace MemBot.Logger;
public class LogFile : LogBase
{
  private readonly string _filePath;
  private readonly StringBuilder _history = new(2500);
  private int _addedLog = 0;
  private const int MaxLogInHistory = 50;
  public LogFile(string filePath = "log.txt")
  {
    var directoryName = Path.GetDirectoryName(filePath);
    if (Directory.Exists(directoryName)) Directory.CreateDirectory(directoryName);
    File.Create(filePath).Close();
    _filePath = filePath;
  }
  protected override void LogConcrete(string info)
  {
    _history.Append($"{info}{Environment.NewLine}");
    Interlocked.Increment(ref _addedLog);
    if (_addedLog >= MaxLogInHistory)
    {
      Interlocked.Exchange(ref _addedLog, 0);
      string logs = _history.ToString();
      _history.Clear();
      Task.Run(() => File.AppendAllText(_filePath, logs));
    }
  }
}