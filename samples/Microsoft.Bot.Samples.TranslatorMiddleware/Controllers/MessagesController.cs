using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Bot.Builder;
using Microsoft.Bot.Builder.Adapters;

namespace Microsoft.Bot.Samples.TranslatorMiddleware.Controllers
{
    [Route("api/[controller]")]
    public class MessagesController : BotController
    {
        public MessagesController(BotFrameworkAdapter bot) : base(bot) { }

        protected override Task OnReceiveActivity(IBotContext context)
        {
            var message = context.Request.AsMessageActivity()?.Text;
            if (message != null)
            {
                context.Reply(@"You just said:")
                    .ShowTyping()
                    .Delay(2000)
                    .Reply(message);
            }

            return Task.CompletedTask;
        }
    }
}
