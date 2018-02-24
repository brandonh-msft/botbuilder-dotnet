﻿// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System.Threading.Tasks;
using Microsoft.Bot.Builder.Adapters;
using Microsoft.Bot.Builder.Middleware;
using Microsoft.Bot.Builder.Storage;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Microsoft.Bot.Builder.Prompts.Tests
{
    [TestClass]
    [TestCategory("Prompts")]
    [TestCategory("Text Prompts")]
    public class TextPromptTests
    {
        [TestMethod]
        public async Task SimpleRecognize()
        {
            TestAdapter adapter = new TestAdapter()
                .Use(new ConversationStateManagerMiddleware(new MemoryStorage()));

            await new TestFlow(adapter, MyTestPrompt)
                .Send("hello")
                .AssertReply("Your Name:")
                .Send("test test test")
                .AssertReply("Passed")
                .AssertReply("test test test")
                .StartTest();
        }

        [TestMethod]
        public async Task MinLenghtViaCustomValidator_Fail()
        {
            TestAdapter adapter = new TestAdapter()
                .Use(new ConversationStateManagerMiddleware(new MemoryStorage()));

            await new TestFlow(adapter, LengthCheckPromptTest)
                .Send("hello")
                .AssertReply("Your Name:")
                .Send("1")
                .AssertReply("Failed")
                .StartTest();
        }
        [TestMethod]
        public async Task MinLenghtViaCustomValidator_Pass()
        {
            TestAdapter adapter = new TestAdapter()
                .Use(new ConversationStateManagerMiddleware(new MemoryStorage()));

            await new TestFlow(adapter, LengthCheckPromptTest)
                .Send("hello")
                .AssertReply("Your Name:")
                .Send("123456")
                .AssertReply("Passed")
                .AssertReply("123456")
                .StartTest();
        }


        [TestMethod]
        public async Task FailOnWhitespace()
        {
            TestAdapter adapter = new TestAdapter()
                .Use(new ConversationStateManagerMiddleware(new MemoryStorage()));

            await new TestFlow(adapter, MyTestPrompt)
                .Send("hello")
                .AssertReply("Your Name:")
                .Send(" ")
                .AssertReply("Failed")
                .StartTest();
        }

        public async Task MyTestPrompt(IBotContext context)
        {
            if (context.State.ConversationProperties["topic"] != "textPromptTest")
            {
                context.State.ConversationProperties["topic"] = "textPromptTest";
                context.PromptText("Your Name:");
            }
            else
            {
                var msg = context.Request.AsMessageActivity();
                if (!string.IsNullOrWhiteSpace(msg.Text))
                {
                    context.Reply("Passed");
                    context.Reply(msg.Text);
                }
                else
                {
                    context.Reply("Failed");
                }
            }
        }

        public async Task LengthCheckPromptTest(IBotContext context)
        {
            if (context.State.ConversationProperties["topic"] != "textPromptTest")
            {
                context.State.ConversationProperties["topic"] = "textPromptTest";
                context.PromptText("Your Name:");
            }
            else
            {
                var (Passed, Value) = await context.ValidatePromptReply(MinLengthValidator);
                if (Passed)
                {
                    context.Reply("Passed");
                    context.Reply(Value);
                }
                else
                {
                    context.Reply("Failed");
                }
            }
        }

        public async Task<(bool Passed, string Value)> MinLengthValidator(IBotContext context, string toValidate)
        {
            return (toValidate.Length > 5, toValidate);
        }
    }
}