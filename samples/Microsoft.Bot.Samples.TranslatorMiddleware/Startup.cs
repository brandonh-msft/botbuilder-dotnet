using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Bot.Builder.Adapters;
using Microsoft.Bot.Builder.Ai;
using Microsoft.Bot.Builder.Middleware;
using Microsoft.Bot.Builder.Storage;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Microsoft.Bot.Samples.TranslatorMiddleware
{
    public partial class Startup
    {
        private readonly LuisRecognizerMiddleware _luis;
        public Startup(IHostingEnvironment env)
        {
            var builder = new ConfigurationBuilder()
                .SetBasePath(env.ContentRootPath)
                .AddJsonFile("appsettings.json", optional: true, reloadOnChange: true)
                .AddJsonFile($"appsettings.{env.EnvironmentName}.json", optional: true)
                .AddJsonFile("appsettings.local.json", optional: true)
                .AddEnvironmentVariables();
            Configuration = builder.Build();

            _luis = new LuisRecognizerMiddleware(
                this.Configuration["MICROSOFT_LUIS_APP_ID"],
                this.Configuration["MICROSOFT_LUIS_APP_PASSWORD"]);
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddSingleton(
                new BotFrameworkAdapter(Configuration)
                    .Use(new UserStateManagerMiddleware(new MemoryStorage()))
                    .Use(new Translator(this.Configuration[@"MICROSOFT_TRANSLATOR_KEY"], "en", GetActiveLanguage, SetActiveLanguage)));

            services.AddMvc();
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IHostingEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            app.UseMvc();
        }
    }
}
