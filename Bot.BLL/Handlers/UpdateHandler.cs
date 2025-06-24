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
    private string EscapeMarkdownV1(string text)
    {
        return text
            .Replace("_", "\\_")
            .Replace("*", "\\*")
            .Replace("`", "\\`")
            .Replace("[", "\\[");
    }

    public async Task HandleUpdateAsync(ITelegramBotClient bot, Update update, CancellationToken token)
    {
        if (update.Message is not { } message)
            return;

        var chatId = message.Chat.Id;
        var state = stateService.GetState(chatId);
        
        if (!string.IsNullOrWhiteSpace(message.Text))
        {
            var lowerText = message.Text.ToLowerInvariant();

            var concerns = new[] {
                "що буде", "документи", "зберігаються", "зберігання", "використання", 
                "навіщо", "чи безпечно", "безпечно", "афера", "обман", "це точно", "scam", "safe" , "using",
                "document", "why", "storage", "who", "what", "how"
            };
            if (concerns.Any(keyword => lowerText.Contains(keyword)))
            {
                await bot.SendTextMessageAsync(chatId,
                    "Your documents are processed *only* for the purpose of issuing an insurance policy. " +
                    "They are *not stored*, " +
                    "*not transferred* to third parties and *automatically deleted* after the registration is complete.\n\n" +
                    "Please continue the registration process by sending a photo of your passport or car registration certificate.",
                    parseMode: Telegram.Bot.Types.Enums.ParseMode.Markdown,
                    cancellationToken: token);
                return;
            }
        }

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

            var extractedText = state switch
            {
                BotState.WaitingForPassport => await docProcessor.ProcessPassportAsync(ms),
                BotState.WaitingForVehicleDoc => await docProcessor.ProcessVehicleDocAsync(ms),
                _ => throw new InvalidOperationException("Unexpected state")
            };

            stateService.SetUserData(chatId, state == BotState.WaitingForPassport ? "passportText" : "vehicleDocText", extractedText);
            
            await bot.SendTextMessageAsync(chatId,
                $"Extracted text:\n\n{EscapeMarkdownV1(extractedText)}\n\nIs this information correct? (yes/no)",
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

            return;
        }

        switch (state)
        {
            case BotState.WaitingForPassport:
            case BotState.WaitingForVehicleDoc:
                if (string.Equals(message.Text, "Yes", StringComparison.OrdinalIgnoreCase))
                {
                    if (state == BotState.WaitingForPassport)
                    {
                        stateService.SetState(chatId, BotState.WaitingForVehicleDoc);
                        await bot.SendTextMessageAsync(chatId,
                            "Great! Now please send a photo of the *vehicle registration certificate*.",
                            parseMode: Telegram.Bot.Types.Enums.ParseMode.Markdown,
                            cancellationToken: token);
                    }
                    else
                    {
                        stateService.SetState(chatId, BotState.WaitingForConfirmation);
                        await bot.SendTextMessageAsync(chatId,
                            "Great! Vehicle document processed successfully.\n\n" +
                            "Please confirm the insurance information is correct to proceed.",
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
                    }
                }
                else if (string.Equals(message.Text, "No", StringComparison.OrdinalIgnoreCase))
                {
                    await bot.SendTextMessageAsync(chatId,
                        state == BotState.WaitingForPassport
                            ? "Please send the passport photo again."
                            : "Please send the vehicle registration certificate again.",
                        cancellationToken: token);
                }
                break;

            case BotState.WaitingForConfirmation:
                if (string.Equals(message.Text, "Yes", StringComparison.OrdinalIgnoreCase))
                {
                    var passportText = stateService.GetUserData(chatId, "passportText");
                    var vehicleDocText = stateService.GetUserData(chatId, "vehicleDocText");

                    var parser = new DocumentParser();
                    var (fullName, vin) = parser.ParseData(passportText, vehicleDocText);

                    stateService.SetUserData(chatId, "clientName", fullName);
                    stateService.SetUserData(chatId, "vin", vin);

                    stateService.SetState(chatId, BotState.WaitingForPriceConfirmation);
                    await bot.SendTextMessageAsync(chatId,
                        "The insurance price is *100 USD*. Do you agree? (yes/no)",
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
                }
                else if (string.Equals(message.Text, "No", StringComparison.OrdinalIgnoreCase))
                {
                    stateService.SetState(chatId, BotState.WaitingForPassport);
                    await bot.SendTextMessageAsync(chatId,
                        "Okay, please start over by sending the passport photo again.",
                        cancellationToken: token);
                }
                break;

            case BotState.WaitingForPriceConfirmation:
                if (string.Equals(message.Text, "Yes", StringComparison.OrdinalIgnoreCase))
                {
                    stateService.SetState(chatId, BotState.Done);

                    var clientName = stateService.GetUserData(chatId, "clientName") ?? "Unknown Name";
                    var vin = stateService.GetUserData(chatId, "vin") ?? "Unknown VIN";

                    var policy = policyGenerator.GeneratePolicy(clientName, vin);

                    await bot.SendTextMessageAsync(chatId,
                        "Congratulations! Here is your insurance policy:\n\n" + policy,
                        cancellationToken: token);
                }
                else if (string.Equals(message.Text, "No", StringComparison.OrdinalIgnoreCase))
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