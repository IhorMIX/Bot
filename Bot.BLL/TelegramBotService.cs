using Telegram.Bot;
using Telegram.Bot.Polling;
using Telegram.Bot.Types;

namespace Bot.BLL;

public class TelegramBotService(string token)
{
    private readonly TelegramBotClient _botClient = new TelegramBotClient(token);

    public void Start()
    {
        var cts = new CancellationTokenSource();

        var receiverOptions = new ReceiverOptions
        {
            AllowedUpdates = []
        };

        _botClient.StartReceiving(
            HandleUpdateAsync,
            HandleErrorAsync,
            receiverOptions,
            cancellationToken: cts.Token
        );

        Console.WriteLine("Telegram bot started...");
    }

    private async Task HandleUpdateAsync(ITelegramBotClient bot, Update update, CancellationToken token)
    {
        if (update.Message is not { } message)
            return;

        var chatId = message.Chat.Id;

        if (message.Text == "/start")
        {
            await _botClient.SendTextMessageAsync(
                chatId: chatId,
                text: "Hi! I will help you get car insurance. Please send a photo of your passport.",
                cancellationToken: token
            );
        }
        else
        {
            await _botClient.SendTextMessageAsync(
                chatId: chatId,
                text: "Send a photo of your passport and registration certificate.",
                cancellationToken: token
            );
        }
    }

    private Task HandleErrorAsync(ITelegramBotClient bot, Exception exception, CancellationToken token)
    {
        Console.WriteLine($"Error: {exception.Message}");
        return Task.CompletedTask;
    }
}