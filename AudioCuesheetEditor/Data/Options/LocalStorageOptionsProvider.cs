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
using Microsoft.JSInterop;
using System.Text.Json;

namespace AudioCuesheetEditor.Data.Options
{
    public class LocalStorageOptionsProvider
    {
        private readonly IJSRuntime _jsRuntime;

        public LocalStorageOptionsProvider(IJSRuntime jsRuntime)
        {
            if (jsRuntime is null)
            {
                throw new ArgumentNullException(nameof(jsRuntime));
            }

            _jsRuntime = jsRuntime;
        }

        public async ValueTask<ApplicationOptions> GetApplicationOptions()
        {
            ApplicationOptions options = new();
            String optionsJson = await _jsRuntime.InvokeAsync<String>("ApplicationOptions.get");
            if (String.IsNullOrEmpty(optionsJson) == false)
            {
                try
                {
                    options = JsonSerializer.Deserialize<ApplicationOptions>(optionsJson);
                    if (options != null)
                    {
                        options.SetDefaultValues();
                    }
                    else
                    {
                        options = new ApplicationOptions();
                    }
                }
                catch (JsonException)
                {
                    //Nothing to do, we can not deserialize
                    options = new ApplicationOptions();
                }
            }
            return options;
        }

        public async Task SaveApplicationOptions(ApplicationOptions options)
        {
            if (options == null)
            {
                throw new ArgumentNullException(nameof(options));
            }
            var serializerOptions = new JsonSerializerOptions
            {
                DefaultIgnoreCondition = System.Text.Json.Serialization.JsonIgnoreCondition.WhenWritingNull
            };
            var optionsJson = JsonSerializer.Serialize(options, serializerOptions);
            await _jsRuntime.InvokeVoidAsync("ApplicationOptions.set", optionsJson);
        }
    }
}
