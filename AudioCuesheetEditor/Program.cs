using AudioCuesheetEditor;
using AudioCuesheetEditor.Controller;
using AudioCuesheetEditor.Extensions;
using AudioCuesheetEditor.Model.UI;
using BlazorDownloadFile;
using Blazorise;
using Blazorise.Bootstrap;
using Howler.Blazor.Components;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using Toolbelt.Blazor.Extensions.DependencyInjection;

var builder = WebAssemblyHostBuilder.CreateDefault(args);
builder.RootComponents.Add<App>("#app");
builder.RootComponents.Add<HeadOutlet>("head::after");

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
builder.Services.AddSingleton<TraceChangeManager>();

builder.Services.AddLogging();
builder.Logging.AddConfiguration(builder.Configuration.GetSection("Logging"));

builder.Services.AddHotKeys();

var host = builder.Build();

await host.SetDefaultCulture();

await builder.Build().RunAsync();
