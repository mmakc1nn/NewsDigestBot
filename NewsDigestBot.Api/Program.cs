using Microsoft.EntityFrameworkCore;
using NewsDigestBot.Infrastructure.Data;
using NewsDigestBot.Api.Services;
using NewsDigestBot.Api.Workers;
using Telegram.Bot;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// ад
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("Postgres")));

// Telegram
var botToken = builder.Configuration["Telegram:BotToken"]!;
builder.Services.AddSingleton<ITelegramBotClient>(new TelegramBotClient(botToken));
builder.Services.AddSingleton<BotService>();
builder.Services.AddHostedService<BotPollingWorker>();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseAuthorization();
app.MapControllers();
app.Run();