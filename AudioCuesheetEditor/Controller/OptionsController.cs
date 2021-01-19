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
using AudioCuesheetEditor.Model.Options;
using AudioCuesheetEditor.Shared.ResourceFiles;
using Microsoft.Extensions.Localization;
using Microsoft.JSInterop;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;

namespace AudioCuesheetEditor.Controller
{
    public class OptionsController
    {
        public const String DefaultCultureName = "en-US";

        private readonly IJSRuntime jsRuntime;

        public OptionsController(IJSRuntime runtime)
        {
            jsRuntime = runtime;
            Options = new ApplicationOptions();
        }

        public ApplicationOptions Options { get; private set; }

        public async Task LoadOptions()
        {
            String optionsJson = await jsRuntime.InvokeAsync<String>("ApplicationOptions.get");
            if (String.IsNullOrEmpty(optionsJson) == false)
            {
                Options = JsonSerializer.Deserialize<ApplicationOptions>(optionsJson);
                Options.SetDefaultValues();
            }
        }

        public async Task SaveOptions()
        {
            String optionsJson = null;
            if (Options != null)
            {
                optionsJson = JsonSerializer.Serialize(Options);
            }
            await jsRuntime.InvokeVoidAsync("ApplicationOptions.set", optionsJson);
        }

        public static IReadOnlyCollection<CultureInfo> AvailableCultures
        {
            get
            {
                var cultures = new List<CultureInfo>
                {
                    new CultureInfo("en-US"),
                    new CultureInfo("de-DE")
                };
                return cultures.AsReadOnly();
            }
        }
    }
}
