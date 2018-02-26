using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using System.Xml.Linq;
using Microsoft.Bot.Builder;
using Microsoft.Bot.Builder.Middleware;
using Microsoft.Bot.Schema;

namespace Microsoft.Bot.Samples.TranslatorMiddleware
{
    public class Translator : IMiddleware, IReceiveActivity, ISendActivity
    {
        private readonly string _translationApiKey;
        private readonly string _botLanguage;
        private readonly Func<IBotContext, string> _getUserLanguage;
        private readonly Func<IBotContext, Task<bool>> _setUserLanguage;

        public Translator(string translationApiKey, string botLanguage, Func<IBotContext, string> getUserLanguage, Func<IBotContext, Task<bool>> setUserLanguage)
        {
            _translationApiKey = translationApiKey;
            _botLanguage = botLanguage;
            _getUserLanguage = getUserLanguage;
            _setUserLanguage = setUserLanguage;
        }

        public async Task ReceiveActivity(IBotContext context, MiddlewareSet.NextDelegate next)
        {
            if (context.Request.Type == ActivityTypes.Message)
            {
                var language = _getUserLanguage(context) ?? _botLanguage;

                var translation = await Translate(context.Request.AsMessageActivity().Text, language, _botLanguage);
                context.Request.AsMessageActivity().Text = translation;

                var languageWasChanged = await _setUserLanguage(context);
                if (!languageWasChanged)
                {   // if what the user said wasn't a directive to change the language (or that directive failed), continue the pipeline
                    await next();
                }
            }
        }

        static HttpClient _httpClient = new HttpClient();
        private async Task<string> Translate(string text, string from, string to)
        {
            if (!from.Equals(to, StringComparison.InvariantCultureIgnoreCase))
            {
                string uri = $@"https://api.microsofttranslator.com/V2/Http.svc/Translate?from={from}&to={to}&text={System.Net.WebUtility.UrlEncode(text)}";

                _httpClient.DefaultRequestHeaders.Add("Ocp-Apim-Subscription-Key", _translationApiKey);
                var result = await _httpClient.GetStringAsync(uri);
                return XElement.Parse(result).Value;
            }
            else
            {
                //if the from and to language are the same 
                return text;
            }
        }

        public async Task SendActivity(IBotContext context, IList<IActivity> activities, MiddlewareSet.NextDelegate next)
        {
            var language = _getUserLanguage(context);
            if (!string.IsNullOrWhiteSpace(language))
            {
                try
                {
                    await Task.WhenAll(
                        activities.Where(a => a.AsMessageActivity() != null)
                            .Select(async activity =>
                            {
                                activity.AsMessageActivity().Text = await Translate(activity.AsMessageActivity().Text, _botLanguage, language);
                            }
                    ));

                }
                catch (Exception err)
                {
                    Trace.TraceError(err.ToString());
                }
            }

            await next();
        }
    }
}
