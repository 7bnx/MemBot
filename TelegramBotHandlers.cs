using Telegram.Bot;
using Telegram.Bot.Exceptions;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using Telegram.Bot.Types.InlineQueryResults;
using Telegram.Bot.Types.InputFiles;
using Telegram.Bot.Types.ReplyMarkups;


namespace MemBot;

internal class TelegramBotHandlers
{
  public static event Func<Mem, Task<bool>> OnAddMem = null!;
  private static readonly Dictionary<long, List<Message>> _messages = new();
  private static readonly MemMediaBuilder _memMediaBuilder = new();
  private const string commandAdd = "/add";
  private const string commandAdded = "/added";
  private const string commandStart = "/start";
  private const string welcomeText = "������!\n" +
                                     "��� ��� ��� �������� �����, �����, ��������, �����";
  private const string enterTagsText = "������� ����, ��������� ������ '#'. ��������:\n" +
                                       "#����� #������ #����";
  private const string loadMediaText = "��������� ���������: ��������, �����, �����\n" +
                                      $"���� ����� ���, �� ������� ������� {commandAdded}";
  private const string addedMemCompleteText = "��� ��������";
  private const string addNewMemText = "������� �����, ������ �������� ����";
  private const string usageText = "Usage:\n" +
                                  $"{commandAdd} - �������� ������\n";
  public static Task ErrorAsync(ITelegramBotClient botClient, Exception exception, CancellationToken cancellationToken)
  {
    var ErrorMessage = exception switch
    {
      ApiRequestException apiRequestException => $"Telegram API Error:\n[{apiRequestException.ErrorCode}]\n{apiRequestException.Message}",
      _ => exception.ToString()
    };

    Console.WriteLine(ErrorMessage);
    return Task.CompletedTask;
  }

  public static async Task UpdateAsync(ITelegramBotClient botClient, Update update, CancellationToken cancellationToken)
  {
    var handler = update.Type switch
    {
      UpdateType.Message => BotOnMessageReceived(botClient, update.Message!),
      _ => UnknownUpdateHandlerAsync(botClient, update)
    };

    try
    {
      await handler;
    }
    catch (Exception exception)
    {
      await ErrorAsync(botClient, exception, cancellationToken);
    }
  }

  private static async Task<Message> TextMessageHandler(ITelegramBotClient botClient, Message message)
  {
    var action = message.Text!.Split(' ')[0] switch
    {
      //"/photo" => SendFile(botClient, message),
      commandAdd => AddCommandHandler(botClient, message),
      commandAdded => AddedCommandHandler(botClient, message),
      commandStart => WelcomeMessage(botClient, message),
      _ => TypedTextSolver(botClient, message)
    };

    return await action;
  }

  private static bool IsMediaMessage(Message message)
    => message.Type == MessageType.Audio || message.Type == MessageType.Document ||
       message.Type == MessageType.Photo || message.Type == MessageType.Video;

  private static async Task<Message> MediaMessageHandler(ITelegramBotClient botClient, Message message)
  {
    var chatId = message.Chat.Id;
    if (_messages.ContainsKey(chatId))
    {
      _messages[chatId].Add(message);
      return await AddedCommandHandler(botClient, message);
    }
    return await SendTextMessageAsync(botClient, message, usageText);
  }

  private static async Task BotOnMessageReceived(ITelegramBotClient botClient, Message message)
  {
    Console.WriteLine($"Receive message type: {message.Type}");
    Message sentMessage = new();
    if (message.Type == MessageType.Text)
      sentMessage = await TextMessageHandler(botClient, message);
    else if (IsMediaMessage(message))
      sentMessage = await MediaMessageHandler(botClient, message);

    Console.WriteLine($"The message was sent with id: {sentMessage.MessageId}");

    static async Task<Message> SendFile(ITelegramBotClient botClient, Message message)
    {
      await botClient.SendChatActionAsync(message.Chat.Id, ChatAction.UploadPhoto);

      const string filePath = @"Lol.jpg";
      using FileStream fileStream = new(filePath, FileMode.Open, FileAccess.Read, FileShare.Read);
      var fileName = filePath.Split(Path.DirectorySeparatorChar).Last();

      return await botClient.SendDocumentAsync(chatId: message.Chat.Id,
                                            new InputOnlineFile(fileStream, fileName),
                                            caption: "Nice Picture");
    }
  }
  static async Task<Message> WelcomeMessage(ITelegramBotClient botClient, Message message)
  {
    await SendTextMessageAsync(botClient, message, welcomeText);
    return await SendTextMessageAsync(botClient, message, usageText);
  }

  static async Task<Message> TypedTextSolver(ITelegramBotClient botClient, Message message)
  {
    var chatId = message.Chat.Id;
    if (_messages.ContainsKey(chatId) && _messages[chatId][0].Text!.Contains(commandAdd))
    {
      if(_messages[chatId].Count == 1)
      {
        _messages[chatId].Add(message);
        return await SendTextMessageAsync(botClient, message, enterTagsText);
      }

      if (_messages[chatId].Count == 2)
      {
        if(MemTag.HasTag(message.Text!)) _messages[chatId].Add(message);
        else return await SendTextMessageAsync(botClient, message, enterTagsText);
      }

      return await SendTextMessageAsync(botClient, message, loadMediaText);
    }
    return await SendTextMessageAsync(botClient, message, usageText);
  }

  private static async Task<Message> AddCommandHandler(ITelegramBotClient botClient, Message message)
  {
    var chatId = message.Chat.Id;
    if (!_messages.ContainsKey(chatId)) _messages.Add(chatId, new List<Message>());
    else _messages[chatId].Clear();

    _messages[chatId].Add(message);

    return await SendTextMessageAsync(botClient, message, addNewMemText);
  }

  private static async Task<Message> AddedCommandHandler(ITelegramBotClient botClient, Message message)
  {
    var chatId = message.Chat.Id;
    if (_messages.ContainsKey(chatId))
    {
      var count = _messages[chatId].Count;
      var isFirstMessageAdd = _messages[chatId].First().Text!.Contains(commandAdd);
      var messages = _messages[chatId];
      _messages.Remove(chatId);
      if (messages.Count >= 3 && messages[2].Type == MessageType.Text && isFirstMessageAdd)
      {
        var mem = await BuildMem(botClient, messages);
        var isAdded = await OnAddMem!.Invoke(mem);
        return await SendTextMessageAsync(botClient, message, isAdded ? addedMemCompleteText : "���-�� �� ���");
      }
    }
    return await SendTextMessageAsync(botClient, message, usageText);
  }

  private static async Task<Mem> BuildMem(ITelegramBotClient botClient, List<Message> messages)
  {
    var listTags = MemTag.ToList(messages[2].Text!);
    Mem mem = new() { Tags = listTags, Text = messages[1].Text! };
    if (messages.Count >= 4)
    {
      var media = await BuildMemMedia(botClient, messages[3]);
      if(media != null) mem.Media.Add(media);
    }
    return mem;
  }

  private static async Task<Message> SendTextMessageAsync(ITelegramBotClient botClient, Message message, string text)
    => await botClient.SendTextMessageAsync(chatId: message.Chat.Id,
                                            text: text,
                                            replyMarkup: new ReplyKeyboardRemove());

  private static string GetFileId(Message message)
  {
    return message.Type switch {
      MessageType.Audio => message.Audio!.FileId,
      MessageType.Document => message.Document!.FileId,
      MessageType.Photo => message.Photo!.LastOrDefault()!.FileId,
      MessageType.Video => message.Video!.FileId,
      _ => string.Empty
    };
  }

  private static async Task<MemMedia> BuildMemMedia(ITelegramBotClient botClient, Message message)
  {
    var fileId = GetFileId(message);
    if (fileId == string.Empty) return null!;

    var file = await botClient.GetFileAsync(fileId);
    using MemoryStream ms = new();
    await botClient.DownloadFileAsync(file.FilePath!, ms);
    if(ms.Length == 0) return null!;
    string extension = "." + file!.FilePath!.Split('.')!.Last();

    var media = _memMediaBuilder.Create(message.Type, extension);
    media.Name = file!.FileId;
    media.Data = ms.ToArray();
    //media.Data = await FileDownloader.FromUrl(GetFileUrl(file));
    media.Size = media.Data.Length;
    return media;
  }

  //private static string GetFileUrl(Telegram.Bot.Types.File file)
    //=> @$"https://api.telegram.org/file/bot{BotToken}/{file.FilePath}";

  private static Task UnknownUpdateHandlerAsync(ITelegramBotClient botClient, Update update)
  {
    Console.WriteLine($"Unknown update type: {update.Type}");
    return Task.CompletedTask;
  }
}