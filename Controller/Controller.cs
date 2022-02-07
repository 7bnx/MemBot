using System.Collections.Concurrent;
using System.Text;
using MemBot.Logger;

namespace MemBot
{
  public class Controller : IController
  {
    private readonly ConcurrentDictionary<string, List<string>> _chatHistory = new();
    public IStorage Storage { get; set; } = null!;
    public IBot Bot { get; set; } = null!;
    public IMediaFactory MediaFactory { get; set; } = null!;
    public ILog Logger { get; set; } = new LogNull();

    private const string CommandAdd = "/add";
    private const string CommandTags = "/tags";
    private const string CommandAdded = "/added";
    private const string CommandStart = "/start";
    private const string CommandHelp = "/help";

    private const string AddNewMemText = "Type name of mem or phrase";
    private const string TypeTagsText = "Type tags with leading '#'.\n" +
                                        "Tags should be separated by '#'\n" +
                                        "Example: #movie #quote #joke";
    private const string LoadMediaText = "Load the media file: image, video, audio\n" +
                                        $"If there is no corresponding media to mem, then type\n" +
                                        $"{CommandAdded}";
    private const string StartText = "Hello!\n" +
                                     "This bot is to store and get mems, phrases, quotes";

    private const string HelpText = "Help:\n" +
                                    "The following commands are supported:\n" +
                                   $"{CommandAdd}\t - add mem\n" + 
                                   $"{CommandTags}\t - show available tags\n" +
                                   $"{CommandHelp}\t - show this info\n\n" +
                                    "To receive mem, type tags with '#'\n" +
                                    "Tags should be separated by '#'\n" +
                                    "Example: #movie #quote #joke";

    private const string MemIsAddedText = "Mem is added";
    private const string MemAddingFailedText = "Mem not added, something went wrong";
    private const string MemByTagsNotFound = "No mem found with the given tags";
    private const string NoTagsFound = "No tags found";
    private const int MaxTagCountToSend = 20;

    public Controller() { }
    public Controller(IBot bot, IStorage storage, IMediaFactory mf)
    {
      Bot = bot;
      Storage = storage;
      MediaFactory = mf;
    }

    public void Start()
    {
      Logger.Log("Bot started");
      Bot.OnReceivedTextMessage += TextMessageReceivedHandler;
      Bot.OnReceivedMediaMessage += MediaMessageReceivedHandler;
      Bot.Run();
    }

    private void TextMessageReceivedHandler(string chat, string message)
    {
      Task.Run(() =>
      {
        Logger.Log($"Received text message from id:{chat}, value: {message}");
        if (string.IsNullOrEmpty(chat) || string.IsNullOrEmpty(message)) return;
        var action = message switch
        {
          CommandAdd => CommandAddHandler(chat),
          CommandHelp => CommandHelpHandler(chat),
          CommandAdded => CommandAddedHandler(chat),
          CommandStart => CommandStartHandler(chat),
          CommandTags => CommandTagHandler(chat),
          _ => UnknownCommandHandler(chat, message)
        };
      });
    }

    private void MediaMessageReceivedHandler(string chat, Media.Types type, string name, Task<MemoryStream> fileData)
    {
      Task.Run(async () => 
      {
        Logger.Log($"Received media message from id:{chat}, type: {type.ToString()}, file name: {name}");
        if (IsAddingComplete(chat, out var messages))
        {
          Media media = MediaFactory.Create(type);
          media.Name = name.Split('.')[0];
          media.Extension = $".{name.Split('.')[1]}";
          var ms = await fileData;
          media.Data = ms.ToArray();
          media.Size = media.Data.Length;
          AddToStorage(chat, messages, media);
        }
        _chatHistory.TryRemove(chat, out _);
      });
    }

    private bool AddToStorage(string chat, List<string> messages, Media media)
    {
      Mem mem = new();
      mem.Text = messages[1];
      mem.Tags.AddRange(Tag.ToArray(messages[2]));
      if (media is not null) mem.Media.Add(media);

      var isAdded = Storage.Add(mem).Result;
      if(isAdded) Bot.Send(chat, MemIsAddedText);
      else Bot.Send(chat, MemAddingFailedText);
      Logger.Log($"Mem adding from id:{chat}, value: {messages[1]}, is added: {isAdded}");
      return isAdded;
    }

    private bool IsAddingComplete(string chat, out List<string> messages)
    {
      var isContain = _chatHistory.TryGetValue(chat, out messages!);
      return isContain && messages?[0] == CommandAdd && messages.Count == 3;
    }
    private bool CommandAddHandler(string chat)
    {
      if (_chatHistory.ContainsKey(chat)) _chatHistory[chat].Clear();
      else _chatHistory.TryAdd(chat, new());
      _chatHistory[chat].Add(CommandAdd);
      Bot.Send(chat, AddNewMemText);
      return true;
    }

    private bool CommandTagHandler(string chat)
    {
      Task.Run(() => {
        var tags = Storage.GetTags().Result;
        StringBuilder sb = new();

        if (tags?.Count == 0)
          sb.Append(NoTagsFound);
        else
        {
          Shuffle(tags!);
          foreach (var tag in tags!.Take(MaxTagCountToSend)) 
            sb.Append($"#{tag.Name} ");
        }
        Bot.Send(chat, sb.ToString());
      });
      _chatHistory.Remove(chat, out _);
      return true;
    }

    private bool CommandHelpHandler(string chat)
    {
      _chatHistory.Remove(chat, out _);
      Bot.Send(chat, HelpText);
      return true;
    }

    private bool CommandAddedHandler(string chat)
    {
      if (IsAddingComplete(chat, out var messages))
         AddToStorage(chat, messages, null!);
      _chatHistory.TryRemove(chat, out _);
      return true;
    }

    private bool CommandStartHandler(string chat)
    {
      _chatHistory.Remove(chat, out _);
      Bot.Send(chat, StartText).ContinueWith((t) => Bot.Send(chat, HelpText));
      return true;
    }

    private bool UnknownCommandHandler(string chat, string message)
    {
      bool isContain = _chatHistory.TryGetValue(chat, out var messages);

      if (isContain) ReceivedTextHandler(chat, message, messages!);
      else if (Tag.HasTags(message))
      {
        Task.Run(async () =>
        {
          var mem = await Storage.GetMem(message);
          if (mem is null) await Bot.Send(chat, MemByTagsNotFound);
          else await Bot.Send(chat, mem);
        });
      }

      return isContain;
    }

    private void ReceivedTextHandler(string chat, string message, List<string> messages)
    {
      if (messages.Count == 0 || messages[0] != CommandAdd) return;

      switch (messages.Count)
      {
        case 1:
          _chatHistory[chat].Add(message);
          Bot.Send(chat, TypeTagsText);
          break;
        case 2:
          if (Tag.HasTags(message))
          {
            _chatHistory[chat].Add(message);
            Bot.Send(chat, LoadMediaText);
          } else Bot.Send(chat, TypeTagsText);
          break;
        default:
          _chatHistory.TryRemove(chat, out _);
          return;
      };
    }

    private void Shuffle<T>(IList<T> list)
    {
      Random random = new();
      int n = list.Count;

      for (int i = n - 1; i > 1; i--)
      {
        int rnd = random.Next(i + 1);

        T value = list[rnd];
        list[rnd] = list[i];
        list[i] = value;
      }
    }
  }
}