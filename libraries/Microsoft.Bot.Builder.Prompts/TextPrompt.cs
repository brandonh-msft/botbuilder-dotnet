using System;
using System.Threading.Tasks;
using Microsoft.Bot.Schema;
using static Microsoft.Bot.Builder.Prompts.PromptValidatorEx;

namespace Microsoft.Bot.Builder.Prompts
{
    /// <summary>
    /// Text Prompt provides a simple mechanism to send text to a user
    /// and validate a response. The default validator passes on any 
    /// non-whitespace string. That behavior is easily changed by deriving
    /// from this class and authoring custom validation behavior. 
    /// 
    /// For simple validation changes, a PromptValidator may be passed in to the 
    /// constructor. If the standard validation passes, the custom PromptValidator
    /// will be called. 
    /// </summary>
    public static class TextPromptEx
    {
        /// <summary>
        /// Creates a new Message, and queues it for sending to the user. 
        /// </summary>
        public static void PromptText(this IBotContext context, string text, string speak = null)
        {
            IMessageActivity activity = MessageFactory.Text(text, speak);
            activity.InputHint = InputHints.ExpectingInput;
            PromptText(context, activity);
        }

        /// <summary>
        /// Creates a new Message Activity, and queues it for sending to the user. 
        /// </summary>
        public static void PromptText(this IBotContext context, IMessageActivity activity) => context.Responses.Add(activity);

        /// <summary>
        /// Used to validate the incoming text, expected on context.Request, is
        /// valid according to the rules defined in the validation steps. 
        /// </summary>        
        /// <returns>Tuple with the following values:
        /// Passed: [true/false]. Indicates if the validation passed or failed.
        /// Value: IF the validation passed, the validated string is retured. 
        /// If the validation failed, the value of this is not defined.
        /// </returns>
        /// <remarks>
        /// The Tuple is returned to allow both value and reference types to be 
        /// validated, allowig this pattern to be used across numeric recognizers
        /// DateTime recoginizers, and other types. Because value types are 
        /// non-nullable, the common pattern of returning null breaks down in 
        /// those scenarios.
        /// If validation is not required, simply request <c>context.Request.AsMessageActivity().Text</c>
        /// to get the user's reply.
        /// </remarks>
        public static async Task<(bool Passed, string Value)> ValidatePromptReply(this IBotContext context, PromptValidator<string, string> validator)
        {
            if (validator == null) throw new ArgumentNullException(nameof(validator));

            BotAssert.ContextNotNull(context);
            BotAssert.ActivityNotNull(context.Request);

            if (context.Request.Type != ActivityTypes.Message)
                throw new InvalidOperationException("No Message to Recognize");

            return await validator(context, context.Request.AsMessageActivity().Text);
        }
    }
}
