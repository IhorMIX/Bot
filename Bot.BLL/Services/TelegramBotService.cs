using System;
using System.Threading;
using Telegram.Bot;
using Telegram.Bot.Polling;
using Telegram.Bot.Types.Enums;
using Bot.BLL.Handlers;
using Bot.BLL.States;
using Bot.BLL.DocumentProcessing;
using Bot.BLL.Policy;

namespace Bot.BLL;

public class TelegramBotService
{
    private readonly TelegramBotClient _botClient;
    private readonly UpdateHandler _updateHandler;
    private CancellationTokenSource _cts;

    public TelegramBotService(string token)
    {
        _botClient = new TelegramBotClient(token);

        var stateService = new UserStateService();
        var docProcessor = new DocumentProcessor();
        var policyGenerator = new PolicyGenerator();

        _updateHandler = new UpdateHandler(stateService, docProcessor, policyGenerator);
    }

    public void Start()
    {
        _cts = new CancellationTokenSource();

        var receiverOptions = new ReceiverOptions
        {
            AllowedUpdates = Array.Empty<UpdateType>()
        };

        _botClient.StartReceiving(
            _updateHandler.HandleUpdateAsync,
            HandleErrorAsync,
            receiverOptions,
            cancellationToken: _cts.Token
        );

        Console.WriteLine("Telegram bot started...");
    }

    private Task HandleErrorAsync(ITelegramBotClient bot, Exception exception, CancellationToken token)
    {
        var errorMessage = exception.Message;
        var exceptionType = exception.GetType().Name;
        Console.WriteLine($"Error type: {exceptionType}");
        Console.WriteLine($"Error message: {errorMessage}");
        Console.WriteLine($"Stack trace: {exception.StackTrace}");
        return Task.CompletedTask;
    }

    public void Stop() => _cts.Cancel();
}