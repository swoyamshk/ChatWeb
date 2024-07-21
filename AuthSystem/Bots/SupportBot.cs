using Microsoft.Bot.Builder;
using Microsoft.Bot.Schema;
using System.Threading;
using System.Threading.Tasks;

public class SupportBot : ActivityHandler
{
    protected override async Task OnMessageActivityAsync(ITurnContext<IMessageActivity> turnContext, CancellationToken cancellationToken)
    {
        var userMessage = turnContext.Activity.Text;
        var botResponse = $"You said: {userMessage}";
        await turnContext.SendActivityAsync(MessageFactory.Text(botResponse), cancellationToken);

        // Store the bot's response in the turn context's state
        turnContext.TurnState.Add("BotResponse", botResponse);
    }
}
