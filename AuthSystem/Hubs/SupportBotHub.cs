using Microsoft.AspNetCore.SignalR;
using Microsoft.Bot.Builder.Integration.AspNet.Core;
using Microsoft.Bot.Builder;
using Microsoft.Bot.Schema;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

public class SupportBotHub : Hub
{
    private readonly IBotFrameworkHttpAdapter _adapter;
    private readonly IConfiguration _configuration;
    private readonly ILogger<SupportBotHub> _logger;

    public SupportBotHub(IBotFrameworkHttpAdapter adapter, IConfiguration configuration, ILogger<SupportBotHub> logger)
    {
        _adapter = adapter ?? throw new ArgumentNullException(nameof(adapter));
        _configuration = configuration ?? throw new ArgumentNullException(nameof(configuration));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task SendMessage(string user, string message)
    {
        _logger.LogInformation("SendMessage called with user: {user}, message: {message}", user, message);

        try
        {
            if (string.IsNullOrEmpty(user))
            {
                throw new ArgumentException("User must not be null or empty.", nameof(user));
            }

            if (string.IsNullOrEmpty(message))
            {
                throw new ArgumentException("Message must not be null or empty.", nameof(message));
            }

            var bot = new SupportBot();
            var activity = new Activity { Text = message, Type = ActivityTypes.Message, From = new ChannelAccount(user) };

            if (!(_adapter is BotAdapter botAdapter))
            {
                throw new InvalidOperationException("Adapter is not a BotAdapter.");
            }

            var turnContext = new TurnContext(botAdapter, activity);

            await bot.OnTurnAsync(turnContext, CancellationToken.None);

            // Retrieve the bot's response from the turn context's state
            if (turnContext.TurnState.TryGetValue("BotResponse", out var botResponse))
            {
                _logger.LogInformation("BotResponse: {botResponse}", botResponse);
                await Clients.All.SendAsync("ReceiveMessage", "Bot", botResponse.ToString());
            }
            else
            {
                _logger.LogWarning("BotResponse not found in TurnState.");
            }

            await Clients.All.SendAsync("ReceiveMessage", user, message);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error in SendMessage");
            throw;
        }
    }
}
