
using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Azure.WebJobs.Host;
using Microsoft.Bot.Builder;
using Microsoft.Bot.Builder.Adapters;
using Microsoft.Bot.Builder.Middleware;
using Microsoft.Bot.Builder.Storage;
using Microsoft.Bot.Schema;
using Newtonsoft.Json;

namespace Microsoft.Bot.Samples.AzureFunction
{
    public static class MyBot
    {
        private static readonly BotFrameworkAdapter _myBot;

        static MyBot()
        {
            _myBot = new BotFrameworkAdapter(Environment.GetEnvironmentVariable(@"MS_APP_ID"), Environment.GetEnvironmentVariable(@"MS_APP_PASSWORD"))
                .Use(new ConversationState<EchoState>(new MemoryStorage(), new StateSettings { LastWriterWins = true, WriteBeforeSend = true }));
        }

        [FunctionName("MyBot")]
        public static async System.Threading.Tasks.Task<IActionResult> RunAsync([HttpTrigger(AuthorizationLevel.Anonymous, "get", "post", Route = null)]HttpRequest req, TraceWriter log)
        {
            log.Info("C# HTTP trigger function processed a request.");

            string requestBody = new StreamReader(req.Body).ReadToEnd();
            var activity = JsonConvert.DeserializeObject<Activity>(requestBody);
            try
            {
                await _myBot.ProcessActivity(req.Headers[@"Authentication"].FirstOrDefault(), activity, BotLogic);

                return new OkResult();
            }
            catch (Exception ex)
            {
                return new ObjectResult(ex) { StatusCode = StatusCodes.Status500InternalServerError };
            }
        }

        private static Task BotLogic(IBotContext arg)
        {
            if (arg.Request.Type == ActivityTypes.Message)
            {
                var msg = arg.Request.AsMessageActivity();
                arg.Reply($@"You said {msg.Text}");
            }
            else if (arg.Request.Type == ActivityTypes.ConversationUpdate)
            {
                var cUpdate = arg.Request.AsConversationUpdateActivity();
                foreach (var m in cUpdate.MembersAdded.Where(a => a.Id != arg.Request.Recipient.Id))
                {
                    arg.Reply($@"Welcome to the echo bot, {m.Name}. Say something and I'll echo it back.");
                }
            }

            return Task.CompletedTask;
        }

        public class EchoState : StoreItem
        {
        }
    }
}
