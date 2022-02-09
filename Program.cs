using MemBot;
using MemBot.Logger;

const string botToken = "Enter bot token";

Controller controller = new ControllerBuilder(new Controller())
                          .SetBot(new TelegramBot(botToken))
                          .SetStorage(new StorageProxy(new MemMatching()))
                          .SetMediaFactory(new MediaFactory())
                          .SetLogger(new LogFile())
                          .Build();

controller.Start();

Console.ReadLine();