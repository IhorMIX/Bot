using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.ReplyMarkups;
using Bot.BLL.States;
using Bot.BLL.DocumentProcessing;
using Bot.BLL.Policy;

namespace Bot.BLL.Handlers;

public class UpdateHandler
{
    private readonly UserStateService _stateService;
    private readonly DocumentProcessor _docProcessor;
    private readonly PolicyGenerator _policyGenerator;

    public UpdateHandler(UserStateService stateService, DocumentProcessor docProcessor, PolicyGenerator policyGenerator)
    {
        _stateService = stateService;
        _docProcessor = docProcessor;
        _policyGenerator = policyGenerator;
    }

    public async Task HandleUpdateAsync(ITelegramBotClient bot, Update update, CancellationToken token)
    {
        if (update.Message is not { } message)
            return;

        var chatId = message.Chat.Id;
        var state = _stateService.GetState(chatId);

        if (message.Text == "/start")
        {
            _stateService.SetState(chatId, BotState.WaitingForPassport);
            await bot.SendTextMessageAsync(chatId,
                "Hello! I'll help you get your car insurance.\n\n Please send a photo of your *passport*.",
                parseMode: Telegram.Bot.Types.Enums.ParseMode.Markdown,
                cancellationToken: token);
            return;
        }
        
        if (message.Photo != null && message.Photo.Any())
        {
            var file = await bot.GetFileAsync(message.Photo.Last().FileId, cancellationToken: token);
            using var ms = new MemoryStream();
            await bot.DownloadFileAsync(file.FilePath, ms, token);
            ms.Seek(0, SeekOrigin.Begin);

            var extractedText = _docProcessor.ProcessDocuments(ms);

            switch (state)
            {
                case BotState.WaitingForPassport:
                    _stateService.SetState(chatId, BotState.WaitingForVehicleDoc);
                    await bot.SendTextMessageAsync(chatId,
                        "Passport received. Recognized text:\n\n" + extractedText +
                        "\n\nNow send a photo of the *technical passport*.",
                        parseMode: Telegram.Bot.Types.Enums.ParseMode.Markdown,
                        cancellationToken: token);
                    break;

                case BotState.WaitingForVehicleDoc:
                    _stateService.SetState(chatId, BotState.WaitingForConfirmation);
                    await bot.SendTextMessageAsync(chatId,
                        "Technical passport received. Recognized text:\n\n" + extractedText +
                        "\n\nConfirm that the data is correct.",
                        replyMarkup: new ReplyKeyboardMarkup(new[]
                        {
                            new[] { new KeyboardButton("Yes"), new KeyboardButton("No") }
                        })
                        {
                            ResizeKeyboard = true,
                            OneTimeKeyboard = true
                        },
                        parseMode: Telegram.Bot.Types.Enums.ParseMode.Markdown,
                        cancellationToken: token);
                    break;

                default:
                    await bot.SendTextMessageAsync(chatId,
                        "Please follow the instructions. Type /start to start over.",
                        cancellationToken: token);
                    break;
            }

            return;
        }
        
        switch (state)
        {
            case BotState.WaitingForConfirmation:
                if (message.Text == "✅ Да")
                {
                    _stateService.SetState(chatId, BotState.WaitingForPriceConfirmation);
                    await bot.SendTextMessageAsync(chatId,
                        "The price of insurance is *100 USD*. Do you agree?",
                        replyMarkup: new ReplyKeyboardMarkup(new[]
                        {
                            new[] { new KeyboardButton("Agree"), new KeyboardButton("Not agree") }
                        })
                        {
                            ResizeKeyboard = true,
                            OneTimeKeyboard = true
                        },
                        parseMode: Telegram.Bot.Types.Enums.ParseMode.Markdown,
                        cancellationToken: token);
                }
                else if (message.Text == "No")
                {
                    _stateService.SetState(chatId, BotState.WaitingForPassport);
                    await bot.SendTextMessageAsync(chatId,
                        "OK, send the passport photo again.",
                        cancellationToken: token);
                }
                break;

            case BotState.WaitingForPriceConfirmation:
                if (message.Text == "Agree")
                {
                    _stateService.SetState(chatId, BotState.Done);

                    // TODO:
                    var policy = _policyGenerator.GeneratePolicy("Ivan Ivanov", "WBA1234567890XYZ");

                    await bot.SendTextMessageAsync(chatId,
                        "Congratulations! Here is your insurance policy:\n\n" + policy,
                        cancellationToken: token);
                }
                else if (message.Text == "Not agree")
                {
                    await bot.SendTextMessageAsync(chatId,
                        "Unfortunately, this is the only available price. If you change your mind, enter /start.",
                        cancellationToken: token);
                }
                break;

            default:
                await bot.SendTextMessageAsync(chatId,
                    "Please follow the instructions. Type /start to start over.",
                    cancellationToken: token);
                break;
        }
    }
}
