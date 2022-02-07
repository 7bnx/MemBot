using Telegram.Bot;
using Telegram.Bot.Exceptions;
using Telegram.Bot.Extensions.Polling;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using Telegram.Bot.Types.InputFiles;
using Telegram.Bot.Types.ReplyMarkups;

namespace MemBot;
public class TelegramBot : IBot
{
  public event Action<string, string> OnReceivedTextMessage = null!;
  public event Action<string, Media.Types, string, Task<MemoryStream>> OnReceivedMediaMessage = null!;
  private readonly string _token;
  private readonly TelegramBotClient _bot;
  public string Token => _token;
  public TelegramBot(string token)
  {
    _token = token;
    _bot = new(_token);
    if (_bot.GetMeAsync().Result.IsBot)
    {

    }
  }
  public void Run()
  {
    ReceiverOptions receiverOptions = new() { AllowedUpdates = { } };
    _bot.StartReceiving(UpdateHandlerAsync,
                        ErrorAsync,
                        receiverOptions);
  }

  public Task Send(string chat, Mem mem)
  {
    if (mem is null) return Task.CompletedTask;
    if (mem.Media?.Count == 0 || mem.Media![0] is null)
    {
      return Send(chat, mem!.Text);
    }
    if (!IsChatAndMessageValid(chat, mem.Text, out long chatId)) return Task.CompletedTask;
    _bot.SendChatActionAsync(chatId, ChatAction.UploadDocument);
    var filePath = mem.Media![0].Path;
    var caption = mem.Text;
    var fileName = mem.Media![0].Name;
    return Task.Run( async() => {
      using FileStream fileStream = new(filePath, FileMode.Open, FileAccess.Read, FileShare.Read);
      var task = (mem.Media[0].Type switch
      {
        Media.Types.Image => await _bot.SendPhotoAsync(chatId: chatId,
                                                    new InputOnlineFile(fileStream, fileName),
                                                    caption: caption),
        Media.Types.Audio => await _bot.SendAudioAsync(chatId: chatId,
                                                    new InputOnlineFile(fileStream, fileName),
                                                    caption: caption),
        Media.Types.Video => await _bot.SendVideoAsync(chatId: chatId,
                                                    new InputOnlineFile(fileStream, fileName),
                                                    caption: caption),
        _ => await _bot.SendDocumentAsync(chatId: chatId,
                                          new InputOnlineFile(fileStream, fileName),
                                          caption: caption),
      });
    });
  }

  public Task Send(string chat, string message)
  {
    return Task.Run(async () => {
      if (!IsChatAndMessageValid(chat, message, out long chatId)) return;
      await _bot.SendTextMessageAsync(chatId: chatId,
                                      text: message,
                                      replyMarkup: new ReplyKeyboardRemove());
    });
  }

  private static bool IsChatAndMessageValid(string chat, string message, out long chatId)
    => !(!long.TryParse(chat, out chatId) || message is null || message.Length == 0);

  private async Task UpdateHandlerAsync(ITelegramBotClient botClient, Update update, CancellationToken cancellationToken)
  {
    var handler = update.Type switch
    {
      UpdateType.Message => MessageReceivedHandlerAsync(botClient, update.Message!),
      _ => UnknownUpdateHandlerAsync(update)
    };

    try
    {
      await handler;
    } catch (Exception exception)
    {
      await ErrorAsync(botClient, exception, cancellationToken);
    }
  }
  private Task MessageReceivedHandlerAsync(ITelegramBotClient botClient, Message message)
  {
    if (message.Type == MessageType.Text && !string.IsNullOrEmpty(message.Text))
      OnReceivedTextMessage?.Invoke($"{message.Chat.Id}", message.Text!);
    else if (IsMediaMessage(message, out var type, out var fileId, out var fileName, out var extension))
    {
      if(!string.IsNullOrEmpty(fileId) && !string.IsNullOrEmpty(fileName) && !string.IsNullOrEmpty(extension))
        OnReceivedMediaMessage?.Invoke($"{message.Chat.Id}", type, $"{fileName}.{extension}", FileData(fileId));
    }
    return Task.CompletedTask;
  }

  private async Task<MemoryStream> FileData(string fileId)
  {
    var file = await _bot.GetFileAsync(fileId);
    using var ms = new MemoryStream();
    await _bot.DownloadFileAsync(file.FilePath!, ms);
    return ms;
  }

  private static bool IsMediaMessage(Message message, out Media.Types type, 
                                     out string fileId, out string fileName, out string extension)
  {
    switch (message.Type)
    {
      case MessageType.Audio:
        type = Media.Types.Audio;
        fileId = message?.Audio?.FileId ?? string.Empty;
        (fileName, extension) = GetMediaNameExtension(message!.Audio!.FileName!);
        break;
      case MessageType.Document:
        type = Media.Types.Document;
        fileId = message.Document?.FileId ?? string.Empty;
        (fileName, extension) = GetMediaNameExtension(message!.Document!.FileName!);
        break;
      case MessageType.Photo:
        type = Media.Types.Image;
        fileId = message.Photo?.LastOrDefault()?.FileId ?? string.Empty; ;
        fileName = fileId;
        extension = "jpg";
        break;
      case MessageType.Video:
        type = Media.Types.Video;
        fileId = message.Video?.FileId ?? string.Empty; ;
        fileName = fileId;
        extension = message.Video?.MimeType?.Split('/')?[^1] ?? string.Empty;
        break;
      default:
        type = default;
        fileId = string.Empty;
        fileName = string.Empty;
        extension = string.Empty;
        return false;
    }
    return true;
  }
  private static Task UnknownUpdateHandlerAsync(Update update)
  {
    Console.WriteLine($"Unknown update type: {update.Type}");
    return Task.CompletedTask;
  }
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

  private static (string fileName, string extension) GetMediaNameExtension(string nameWithExtension)
  {
    if(string.IsNullOrEmpty(nameWithExtension)) return (fileName: string.Empty, extension: string.Empty);
    var arr = nameWithExtension.Split('.', StringSplitOptions.RemoveEmptyEntries);
    if(arr == null || arr.Length != 2) return (fileName: string.Empty, extension: string.Empty);
    return (fileName : arr[0], extension : arr[1]);
  }
}