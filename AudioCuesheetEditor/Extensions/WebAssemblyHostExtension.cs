//This file is part of AudioCuesheetEditor.

//AudioCuesheetEditor is free software: you can redistribute it and/or modify
//it under the terms of the GNU General Public License as published by
//the Free Software Foundation, either version 3 of the License, or
//(at your option) any later version.

//AudioCuesheetEditor is distributed in the hope that it will be useful,
//but WITHOUT ANY WARRANTY; without even the implied warranty of
//MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
//GNU General Public License for more details.

//You should have received a copy of the GNU General Public License
//along with Foobar.  If not, see
//<http: //www.gnu.org/licenses />.
using AudioCuesheetEditor.Controller;
using AudioCuesheetEditor.Data.Options;
using AudioCuesheetEditor.Model.Options;
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
        public async static Task SetDefaultCulture(this WebAssemblyHost host)
        {
            var localStorageOptionsProvider = host.Services.GetRequiredService<LocalStorageOptionsProvider>();
            var options = await localStorageOptionsProvider.GetOptions<ApplicationOptions>();

            CultureInfo.DefaultThreadCurrentCulture = options.Culture;
            CultureInfo.DefaultThreadCurrentUICulture = options.Culture;
        }
    }
}
