using Bot.BLL;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddSingleton<TelegramBotService>(provider =>
{
    var token = builder.Configuration["TelegramBotToken"];
    var mindeeApiKey = builder.Configuration["MindeeApiKey"];
    return new TelegramBotService(token, mindeeApiKey);
});

var app = builder.Build();

var telegramService = app.Services.GetRequiredService<TelegramBotService>();
telegramService.Start();

app.MapGet("/", () => "Telegram Bot is running...");
app.Run();