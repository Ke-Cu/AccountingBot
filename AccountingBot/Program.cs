// See https://aka.ms/new-console-template for more information
using AccountingBot;
using AccountingBot.HttpApi;
using AccountingBot.Models;
using Microsoft.Extensions.DependencyInjection;
using System.Text.Json;

var services = new ServiceCollection();
services.AddHttpApi<ILoginApi>();
services.AddHttpApi<IChatApi>();
using var provider = services.BuildServiceProvider();
var config = JsonSerializer.Deserialize<BotConfig>(File.ReadAllText("config.json"));

await new AccountingHandler(provider, config).StartAsync();

