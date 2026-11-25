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
using AudioCuesheetEditor.Model.Utility;
using AudioCuesheetEditor.Services.Options;
using System.Globalization;
using System.Linq.Expressions;
using System.Reflection;

namespace AudioCuesheetEditor.Services.UI;

public class ApplicationOptionsTimeSpanParser
{
    readonly ILocalStorageOptionsProvider _localStorageOptionsProvider;

    ApplicationOptions? _applicationOptions;
    readonly Task? _initTask;
    bool _disposedValue;

    public ApplicationOptionsTimeSpanParser(ILocalStorageOptionsProvider localStorageOptionsProvider)
    {
        _localStorageOptionsProvider = localStorageOptionsProvider;
        _localStorageOptionsProvider.OptionSaved += LocalStorageOptionsProvider_OptionSaved;
        _initTask = InitAsync();
    }

    public async Task TimespanTextChanged<T, TProperty>(T entity, Expression<Func<T, TProperty>> expression, string value)
    {
        if (expression.Body is not MemberExpression memberExpression)
        {
            throw new ArgumentException("'expression' should be a member expression");
        }
        await EnsureInitializedAsync();
        var result = TimeSpanUtility.ParseTimeSpan(value, _applicationOptions?.TimeSpanFormat);
        switch (memberExpression.Member.MemberType)
        {
            case MemberTypes.Property:
                ((PropertyInfo)memberExpression.Member).SetValue(entity, result);
                break;
            default:
                throw new NotImplementedException();
        }
    }

    public async Task<TimeSpan?> GetTimespanAsync(string value)
    {
        await EnsureInitializedAsync();
        var result = TimeSpanUtility.ParseTimeSpan(value, _applicationOptions?.TimeSpanFormat);
        return result;
    }

    public string? GetTimespanFormatted(TimeSpan? timeSpan)
    {
        string? formatted = null;
        EnsureInitializedAsync().GetAwaiter().GetResult();
        if (timeSpan.HasValue)
        {
            try
            {
                formatted = timeSpan.Value.ToString(_applicationOptions?.DisplayTimeSpanFormat, CultureInfo.CurrentCulture);
            }
            catch (FormatException)
            {
                formatted = timeSpan.Value.ToString();
            }
        }
        return formatted;
    }

    protected virtual void Dispose(bool disposing)
    {
        if (!_disposedValue)
        {
            if (disposing)
            {
                // Verwalteten Zustand (verwaltete Objekte) bereinigen
                _localStorageOptionsProvider.OptionSaved -= LocalStorageOptionsProvider_OptionSaved;
            }

            _disposedValue = true;
        }
    }

    async Task InitAsync() => _applicationOptions ??= await _localStorageOptionsProvider.GetOptionsAsync<ApplicationOptions>();

    async Task EnsureInitializedAsync()
    {
        if (_initTask != null)
        {
            await _initTask;
        }
    }

    void LocalStorageOptionsProvider_OptionSaved(object? sender, IOptions options)
    {
        if (options is ApplicationOptions applicationOption)
        {
            _applicationOptions = applicationOption;
        }
    }
}
