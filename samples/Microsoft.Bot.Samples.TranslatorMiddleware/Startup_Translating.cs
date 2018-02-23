using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Bot.Builder;
using Microsoft.Bot.Schema;

namespace Microsoft.Bot.Samples.TranslatorMiddleware
{
    public partial class Startup
    {
        private static readonly Dictionary<string, string> _languageMap = new Dictionary<string, string> {
            { "danish", "da" },
            { "dutch", "nl" },
            { "english", "en" },
            { "finnish", "fi" },
            { "french", "fr" },
            { "german", "de" },
            { "greek", "el" },
            { "italian", "it" },
            { "japanese", "ja" },
            { "norwegian", "no" },
            { "polish", "pl" },
            { "portuguese", "pt" },
            { "russian", "ru" },
            { "spanish", "es" },
            { "swedish", "sv" },
            { "turkish", "tr" }
        };

        private bool IsSupportedLanguage(string language) => _languageMap.ContainsKey(language.ToLowerInvariant());
        private string GetLanguageCode(string language) => _languageMap[language];
        private void SetLanguage(IBotContext context, string language) => context.State.UserProperties[@"translateTo"] = GetLanguageCode(language);

        private async Task<bool> SetActiveLanguage(IBotContext context)
        {
            var intents = await _luis.Recognize(context);

            var intent = intents?.FirstOrDefault();
            if (intent?.Name == @"changeLanguage")
            {
                dynamic entity = intent.Entities?.FirstOrDefault();

                if (entity?.Type == @"language::toLanguage")
                {
                    var entityValue = entity.ValueAs<string>();
                    if (!string.IsNullOrWhiteSpace(entityValue)
                        && IsSupportedLanguage(entityValue))
                    {
                        SetLanguage(context, entityValue);
                        context.Reply($@"Changing your language to {entityValue}");
                    }
                    else
                    {
                        context.Reply($@"{entityValue} is not a supported language.");
                    }
                }
                else
                {
                    context.Reply("You have to tell me what language to translate to!");
                }

                //intercepts message
                return true;
            }

            return false;
        }

        private string GetActiveLanguage(IBotContext context)
        {
            if (context.Request.Type == ActivityTypes.Message
                && context.State.UserProperties.ContainsKey(@"translateTo"))
            {
                return (string)context.State.UserProperties[@"translateTo"];
            }

            return null;
        }
    }
}
