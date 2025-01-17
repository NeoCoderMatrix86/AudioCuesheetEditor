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
using AudioCuesheetEditor.Model.Entity;
using AudioCuesheetEditor.Model.Options;
using Microsoft.JSInterop;
using System.Linq.Expressions;
using System.Reflection;
using System.Text.Json;

namespace AudioCuesheetEditor.Data.Options
{
    //TODO: Make the service load and inject the application options, leading to options beeing available on all components
    //TODO: Also make the application options with sub parts (for record, import, etc.)
    //TODO: Make one dialog for options
    public class LocalStorageOptionsProvider(IJSRuntime jsRuntime): ILocalStorageOptionsProvider
    {
        public event EventHandler<IOptions>? OptionSaved;

        private readonly IJSRuntime _jsRuntime = jsRuntime;

        private readonly JsonSerializerOptions SerializerOptions = new()
        {
            DefaultIgnoreCondition = System.Text.Json.Serialization.JsonIgnoreCondition.WhenWritingNull
        };

        public async Task<T> GetOptions<T>() where T : IOptions
        {
            var type = typeof(T);
            IOptions? options = (IOptions?)Activator.CreateInstance(type);
            String optionsJson = await _jsRuntime.InvokeAsync<String>("AppSettings.get", type.Name);
            if (String.IsNullOrEmpty(optionsJson) == false)
            {
                try
                {
                    options = JsonSerializer.Deserialize<T>(optionsJson);
                    options ??= Activator.CreateInstance<T>();
                }
                catch (JsonException)
                {
                    //nothing to do, we can not deserialize
                    options = Activator.CreateInstance<T>();
                }
            }
            if (options != null)
            {
                return (T)options;
            }
            else
            {
                return default!;
            }
        }

        public async Task SaveOptions(IOptions options)
        {
            var optionsJson = JsonSerializer.Serialize<object>(options, SerializerOptions);
            await _jsRuntime.InvokeVoidAsync("AppSettings.set", options.GetType().Name, optionsJson);
            OptionSaved?.Invoke(this, options);
        }

        public async Task SaveOptionsValue<T>(Expression<Func<T, object>> propertyExpression, object value) where T : class, IOptions, new()
        {
            var options = await GetOptions<T>();
            Boolean saveOptions = true;
            PropertyInfo? propertyInfo = null;
            if (propertyExpression.Body is MemberExpression memberExpression)
            {
                propertyInfo = memberExpression.Member as PropertyInfo;
            }
            else if (propertyExpression.Body is UnaryExpression unaryExpression && unaryExpression.Operand is MemberExpression unaryMemberExpression)
            {
                propertyInfo = unaryMemberExpression.Member as PropertyInfo;
            }
            if (propertyInfo != null)
            {
                propertyInfo.SetValue(options, Convert.ChangeType(value, propertyInfo.PropertyType));
                if (options is IValidateable validateable)
                {
                    saveOptions = validateable.Validate(propertyInfo.Name).Status != ValidationStatus.Error;
                }
            }
            else
            {
                throw new ArgumentException("The provided expression does not reference a valid property.");
            }
            if (saveOptions)
            {
                await SaveOptions(options);
            }
        }
    }
}
