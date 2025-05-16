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
            bool saveOptions = true;
            if (options is IValidateable validateable)
            {
                saveOptions = validateable.Validate().Status != ValidationStatus.Error;
            }
            if (saveOptions)
            {
                var optionsJson = JsonSerializer.Serialize<object>(options, SerializerOptions);
                await _jsRuntime.InvokeVoidAsync("AppSettings.set", options.GetType().Name, optionsJson);
                OptionSaved?.Invoke(this, options);
            }
        }

        public async Task SaveOptionsValue<T>(Expression<Func<T, object?>> propertyExpression, object? value) where T : class, IOptions, new()
        {
            var options = await GetOptions<T>();
            PropertyInfo? propertyInfo = null;
            object? targetObject = options;

            if (propertyExpression.Body is MemberExpression memberExpression)
            {
                propertyInfo = ResolveNestedProperty(memberExpression, ref targetObject);
            }
            else if (propertyExpression.Body is UnaryExpression unaryExpression && unaryExpression.Operand is MemberExpression unaryMemberExpression)
            {
                propertyInfo = ResolveNestedProperty(unaryMemberExpression, ref targetObject);
            }

            if (propertyInfo != null && targetObject != null)
            {
                propertyInfo.SetValue(targetObject, Convert.ChangeType(value, propertyInfo.PropertyType));
            }
            else
            {
                throw new ArgumentException("The provided expression does not reference a valid property.");
            }
            await SaveOptions(options);
        }

        public async Task SaveNestedOptionValue<T, TNested, TValue>(Expression<Func<T, TNested>> nestedPropertyExpression, Expression<Func<TNested, TValue>> valuePropertyExpression, TValue value) where T : class, IOptions, new()
        {
            var options = await GetOptions<T>();

            if (nestedPropertyExpression.Body is not MemberExpression memberExpression)
            {
                throw new ArgumentException("The provided nested property expression does not reference a member!");
            }

            var nestedProperty = typeof(T).GetProperty(memberExpression.Member.Name) ?? throw new ArgumentException("The provided nested property expression does not reference a valid property.");
            var nestedInstance = nestedProperty.GetValue(options) ?? throw new InvalidOperationException("The nested property is null.");
            var valueProperty = ResolveNestedProperty(valuePropertyExpression.Body as MemberExpression, ref nestedInstance) ?? throw new ArgumentException("The provided value property expression does not reference a valid property.");
            valueProperty.SetValue(nestedInstance, Convert.ChangeType(value, valueProperty.PropertyType));

            await SaveOptions(options);
        }

        private static PropertyInfo? ResolveNestedProperty(MemberExpression? memberExpression, ref object? targetObject)
        {
            PropertyInfo? propertyInfo = null;
            var members = new Stack<MemberExpression>();

            while (memberExpression != null)
            {
                members.Push(memberExpression);
                if (memberExpression.Expression is MemberExpression parentMember)
                {
                    memberExpression = parentMember;
                }
                else
                {
                    memberExpression = null;
                }
            }

            while (members.Count > 0 && targetObject != null)
            {
                memberExpression = members.Pop();
                propertyInfo = targetObject.GetType().GetProperty(memberExpression.Member.Name);
                if (propertyInfo != null)
                {
                    if (members.Count > 0)
                    {
                        targetObject = propertyInfo.GetValue(targetObject);
                    }
                }
            }

            return propertyInfo;
        }
    }
}
