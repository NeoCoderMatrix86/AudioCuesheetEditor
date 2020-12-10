using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.JSInterop;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;

namespace AudioCuesheetEditor.Extensions
{
    public static class WebAssemblyHostExtension
    {
        public static readonly CultureInfo DefaultCultureInfo = new CultureInfo("en-US");

        public async static Task SetDefaultCulture(this WebAssemblyHost host)
        {
            var jsInterop = host.Services.GetRequiredService<IJSRuntime>();
            var result = await jsInterop.InvokeAsync<string>("blazorCulture.get");

            CultureInfo culture = DefaultCultureInfo;

            if (result != null)
            {
                culture = new CultureInfo(result);
            }

            CultureInfo.DefaultThreadCurrentCulture = culture;
            CultureInfo.DefaultThreadCurrentUICulture = culture;
        }
    }
}
