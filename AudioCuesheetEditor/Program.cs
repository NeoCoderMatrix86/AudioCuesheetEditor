using AudioCuesheetEditor.Controller;
using AudioCuesheetEditor.Extensions;
using BlazorDownloadFile;
using Blazorise;
using Blazorise.Bootstrap;
using Howler.Blazor.Components;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Toolbelt.Blazor.Extensions.DependencyInjection;

namespace AudioCuesheetEditor
{
    public class Program
    {
        public static async Task Main(string[] args)
        {
            var builder = WebAssemblyHostBuilder.CreateDefault(args);
            builder.RootComponents.Add<App>("app");

            builder.Services.AddScoped(sp => new HttpClient { BaseAddress = new Uri(builder.HostEnvironment.BaseAddress) });

            builder.Services.AddBlazorise(options =>
            {
                options.ChangeTextOnKeyPress = true;
                options.DelayTextOnKeyPress = true;
                options.DelayTextOnKeyPressInterval = 300;
            })
            .AddBootstrapProviders();

            builder.Services.AddScoped<IHowl, Howl>();
            builder.Services.AddScoped<IHowlGlobal, HowlGlobal>();

            builder.Services.AddBlazorDownloadFile();

            builder.Services.AddScoped<CuesheetController>();
            builder.Services.AddScoped<OptionsController>();

            builder.Services.AddSingleton<SessionStateContainer>();
            builder.Services.AddSingleton<ILoggerProvider, LoggerProvider>();
            builder.Services.AddLogging();
            ConfigureLogging(builder);

            builder.Services.AddHotKeys();

            var host = builder.Build();

            await host.SetDefaultCulture();

            await host.RunAsync();
        }

        private static void ConfigureLogging(WebAssemblyHostBuilder builder, string section = "Logging")
        {
            builder.Logging.AddConfiguration(builder.Configuration.GetSection(section));
        }
    }
}
