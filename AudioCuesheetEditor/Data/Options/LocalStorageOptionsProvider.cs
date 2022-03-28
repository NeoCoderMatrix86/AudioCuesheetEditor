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
        //TODO: Event for saving options in order to reload them in different dialogs
        private readonly IJSRuntime _jsRuntime;

        public LocalStorageOptionsProvider(IJSRuntime jsRuntime)
        {
            if (jsRuntime is null)
            {
                throw new ArgumentNullException(nameof(jsRuntime));
            }

            _jsRuntime = jsRuntime;
        }

        public async ValueTask<T> GetOptions<T>() where T : IOptions
        {
            var type = typeof(T);
            IOptions options = (IOptions)Activator.CreateInstance(type);
            String optionsJson = await _jsRuntime.InvokeAsync<String>(String.Format("{0}.get", type.Name));
            if (String.IsNullOrEmpty(optionsJson) == false)
            {
                try
                {
                    options = (IOptions)JsonSerializer.Deserialize(optionsJson, typeof(T));
                    if (options != null)
                    {
                        options.SetDefaultValues();
                    }
                    else
                    {
                        options = (IOptions)Activator.CreateInstance(typeof(T));
                    }
                }
                catch (JsonException)
                {
                    //nothing to do, we can not deserialize
                    options = (IOptions)Activator.CreateInstance(typeof(T));
                }
            }
            return (T)options;
        }

        public async Task SaveOptions(IOptions options)
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
            await _jsRuntime.InvokeVoidAsync(String.Format("{0}.set", options.GetType().Name), optionsJson);
        }
    }
}
