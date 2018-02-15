// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Bot.Builder;
using Microsoft.Bot.Builder.BotFramework;
using Microsoft.Bot.Schema;

namespace Microsoft.Bot.Samples.Simplified.Asp
{
    public abstract class BotController : Controller
    {
        protected readonly BotFrameworkAdapter _adapter;

        public BotController(Builder.Bot bot)
        {
            _adapter = (BotFrameworkAdapter)bot.Adapter;
            bot.OnReceive(BotReceiveHandler);
        }

        protected virtual Task BotReceiveHandler(IBotContext context) => OnReceiveActivity(context);

        protected abstract Task OnReceiveActivity(IBotContext context);

        [HttpPost]
        public virtual async Task<IActionResult> Post([FromBody]Activity activity)
        {
            try
            {
                await _adapter.Receive(this.Request.Headers["Authorization"].FirstOrDefault(), activity);
                return this.Ok();
            }
            catch (UnauthorizedAccessException)
            {
                return this.Unauthorized();
            }
        }
    }
}
