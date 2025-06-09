using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.ReplyMarkups;
using Bot.BLL.States;
using Bot.BLL.DocumentProcessing;
using Bot.BLL.Policy;

namespace Bot.BLL.Handlers;

public class UpdateHandler(
    UserStateService stateService,
    DocumentProcessor docProcessor,
    PolicyGenerator policyGenerator)
{
    public async Task HandleUpdateAsync(ITelegramBotClient bot, Update update, CancellationToken token)
    {
        if (update.Message is not { } message)
            return;

        var chatId = message.Chat.Id;
        var state = stateService.GetState(chatId);

        if (message.Text == "/start")
        {
            stateService.SetState(chatId, BotState.WaitingForPassport);
            await bot.SendTextMessageAsync(chatId,
                "Hello! I will help you get car insurance.\n\nPlease send a photo of your *passport*.",
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

            var extractedText = docProcessor.ProcessDocuments(ms);

            switch (state)
            {
                case BotState.WaitingForPassport:
                    stateService.SetState(chatId, BotState.WaitingForVehicleDoc);
                    await bot.SendTextMessageAsync(chatId,
                        "Passport received. Extracted text:\n\n" + extractedText +
                        "\n\nNow please send a photo of the *vehicle registration certificate*.",
                        parseMode: Telegram.Bot.Types.Enums.ParseMode.Markdown,
                        cancellationToken: token);
                    break;

                case BotState.WaitingForVehicleDoc:
                    stateService.SetState(chatId, BotState.WaitingForConfirmation);
                    await bot.SendTextMessageAsync(chatId,
                        "Vehicle document received. Extracted text:\n\n" + extractedText +
                        "\n\nPlease confirm that the data is correct.",
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
                        "Please follow the instructions. Type /start to begin again.",
                        cancellationToken: token);
                    break;
            }

            return;
        }
        
        switch (state)
        {
            case BotState.WaitingForConfirmation:
                if (message.Text == "Yes")
                {
                    stateService.SetState(chatId, BotState.WaitingForPriceConfirmation);
                    await bot.SendTextMessageAsync(chatId,
                        "The insurance price is *100 USD*. Do you agree?",
                        replyMarkup: new ReplyKeyboardMarkup(new[]
                        {
                            new[] { new KeyboardButton("Agree"), new KeyboardButton("Disagree") }
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
                    stateService.SetState(chatId, BotState.WaitingForPassport);
                    await bot.SendTextMessageAsync(chatId,
                        "Okay, please send the passport photo again.",
                        cancellationToken: token);
                }
                break;

            case BotState.WaitingForPriceConfirmation:
                if (message.Text == "Agree")
                {
                    stateService.SetState(chatId, BotState.Done);

                    // TODO:
                    var policy = policyGenerator.GeneratePolicy("Ivan Ivanov", "WBA1234567890XYZ");

                    await bot.SendTextMessageAsync(chatId,
                        "Congratulations! Here is your insurance policy:\n\n" + policy,
                        cancellationToken: token);
                }
                else if (message.Text == "Disagree")
                {
                    await bot.SendTextMessageAsync(chatId,
                        "Unfortunately, this is the only available price. If you change your mind, type /start.",
                        cancellationToken: token);
                }
                break;

            default:
                await bot.SendTextMessageAsync(chatId,
                    "Please follow the instructions. Type /start to begin again.",
                    cancellationToken: token);
                break;
        }
    }
}
