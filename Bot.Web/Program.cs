using Bot.BLL;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddSingleton<TelegramBotService>(provider =>
{
    var token = builder.Configuration["TelegramBotToken"];
    return new TelegramBotService(token);
});

var app = builder.Build();

var telegramService = app.Services.GetRequiredService<TelegramBotService>();
telegramService.Start();

app.MapGet("/", () => "Telegram Bot is running...");
app.Run();