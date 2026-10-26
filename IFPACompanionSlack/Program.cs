using IFPACompanionSlack.Settings;
using IfpaSlackBot.Handlers;
using Microsoft.Extensions.DependencyInjection;
using PinballApi;
using SlackNet.AspNetCore;
using SlackNet.WebApi;
using System.Globalization;

//Culture is set explicitly because the IFPA values returned are in US Dollars
CultureInfo.DefaultThreadCurrentCulture = new CultureInfo("en-US");

var builder = WebApplication.CreateBuilder(args);

var slackSettings = builder.Configuration.GetSection("Slack").Get<SlackSettings>();
var pinballApiSettings = builder.Configuration.GetSection("PinballApi").Get<PinballApiSettings>();

builder.Services.AddScoped<PinballRankingApiV2>(x => new PinballRankingApiV2(pinballApiSettings.IFPAApiKey));
builder.Services.AddScoped<PinballRankingApiV1>(x => new PinballRankingApiV1(pinballApiSettings.IFPAApiKey));
builder.Services.AddScoped<SlackSettings>(x => slackSettings);
builder.Services.AddScoped<IOAuthV2Api, OAuthV2Api>();

builder.Services.AddControllers();

builder.Services.AddSlackNet(c => c
    // Configure the tokens used to authenticate with Slack
    .UseApiToken(slackSettings.ApiToken) // This gets used by the API client
    .UseAppLevelToken(slackSettings.AppLevelToken) // This gets used by the socket mode client

    // The signing secret ensures that SlackNet only handles requests from Slack
    .UseSigningSecret(slackSettings.SigningSecret)

    // Register your Slack handlers here
    .RegisterSlashCommandHandler<IfpaCommandHandler>(IfpaCommandHandler.SlashCommand)

    // Socket mode opens an outbound connection to Slack, so the bot needs no public URL
    .UseSocketMode(true)
);


var app = builder.Build();

app.UseSlackNet();

app.MapGet("/", () => "Hello, Slack!");
app.MapControllers();

app.Run();