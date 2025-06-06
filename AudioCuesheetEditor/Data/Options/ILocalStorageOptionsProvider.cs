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
using AudioCuesheetEditor.Model.Options;
using System.Linq.Expressions;

namespace AudioCuesheetEditor.Data.Options
{
    public interface ILocalStorageOptionsProvider
    {
        event EventHandler<IOptions>? OptionSaved;
        Task<T> GetOptionsAsync<T>() where T : IOptions;
        Task SaveOptionsAsync(IOptions options);
        Task SaveOptionsValueAsync<T>(Expression<Func<T, object?>> propertyExpression, object? value) where T : class, IOptions, new();
        Task SaveNestedOptionValueAsync<T, TNested, TValue>(Expression<Func<T, TNested>> nestedPropertyExpression, Expression<Func<TNested, TValue>> valuePropertyExpression, TValue value) where T : class, IOptions, new();
    }
}
