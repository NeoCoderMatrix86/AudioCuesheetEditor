﻿//This file is part of AudioCuesheetEditor.

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
using AudioCuesheetEditor.Data.Options;
using AudioCuesheetEditor.Model.Options;
using AudioCuesheetEditor.Model.Utility;
using System.Linq.Expressions;
using System.Reflection;

namespace AudioCuesheetEditor.Services.UI
{
    public class ApplicationOptionsTimeSpanParser
    {
        private readonly LocalStorageOptionsProvider _localStorageOptionsProvider;

        private ApplicationOptions? applicationOptions;
        private bool disposedValue;

        public ApplicationOptionsTimeSpanParser(LocalStorageOptionsProvider localStorageOptionsProvider)
        {
            _localStorageOptionsProvider = localStorageOptionsProvider;
            _localStorageOptionsProvider.OptionSaved += LocalStorageOptionsProvider_OptionSaved;
            Task.Run(InitAsync);
        }

        public async Task TimespanTextChanged<T, TProperty>(T entity, Expression<Func<T, TProperty>> expression, String value)
        {
            if (expression.Body is not MemberExpression memberExpression)
            {
                throw new ArgumentException("'expression' should be a member expression");
            }
            if (applicationOptions == null)
            {
                await InitAsync();
            }
            TimeSpan? result = TimeSpanUtility.ParseTimeSpan(value, applicationOptions?.TimeSpanFormat);
            switch (memberExpression.Member.MemberType)
            {
                case MemberTypes.Property:
                    ((PropertyInfo)memberExpression.Member).SetValue(entity, result);
                    break;
                default:
                    throw new NotImplementedException();
            }
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing)
                {
                    // Verwalteten Zustand (verwaltete Objekte) bereinigen
                    _localStorageOptionsProvider.OptionSaved -= LocalStorageOptionsProvider_OptionSaved;
                }

                disposedValue = true;
            }
        }

        private async Task InitAsync()
        {
            applicationOptions ??= await _localStorageOptionsProvider.GetOptions<ApplicationOptions>();
        }

        private void LocalStorageOptionsProvider_OptionSaved(object? sender, IOptions options)
        {
            if (options is ApplicationOptions applicationOption)
            {
                applicationOptions = applicationOption;
            }
        }
    }
}