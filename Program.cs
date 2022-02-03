using MemBot;
using Telegram.Bot;
using Telegram.Bot.Extensions.Polling;

const string botToken = "Enter bot token";

TelegramBotClient bot = new(botToken);
ReceiverOptions receiverOptions = new() { AllowedUpdates = { } };
bot.StartReceiving(TelegramBotHandlers.UpdateAsync,
                   TelegramBotHandlers.ErrorAsync,
                   receiverOptions);

while (true) Thread.Sleep(1);
