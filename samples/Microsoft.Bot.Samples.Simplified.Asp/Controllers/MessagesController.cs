// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Bot.Builder;
using Microsoft.Bot.Schema;

namespace Microsoft.Bot.Samples.Simplified.Asp.Controllers
{
    [Route("api/[controller]")]
    public class MessagesController : BotController
    {
        public MessagesController(Builder.Bot bot) : base(bot) { }

        protected override Task OnReceiveActivity(IBotContext context)
        {
            if (context.Request.Type == ActivityTypes.Message)
            {
                var activity = context.Request.AsMessageActivity();
                long turnNumber = context.State.Conversation["turnNumber"] ?? 0;
                context.State.Conversation["turnNumber"] = ++turnNumber;
                context.Reply($"[{turnNumber}] echo: {activity.Text}");
            }
            else if (context.Request.Type == ActivityTypes.ConversationUpdate)
            {
                var actvity = context.Request.AsConversationUpdateActivity();
                foreach (var newMember in actvity.MembersAdded)
                {
                    if (newMember.Id != actvity.Recipient.Id)
                    {
                        context.Reply("Hello and welcome to the echo bot.");
                    }
                }
            }

            return Task.CompletedTask;
        }
    }
}
