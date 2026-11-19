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
using System.Linq.Expressions;
using System.Reflection;
using AudioCuesheetEditor.Model.Entity;
using Microsoft.Extensions.Localization;

namespace AudioCuesheetEditor.Services.Validation;

public class ValidationService(IStringLocalizer<ValidationMessage> localizer)
{
    readonly IStringLocalizer<ValidationMessage> _localizer = localizer;
    public Func<object, string, Task<IEnumerable<string>>> ValidateProperty => (model, propertyName) => Task.FromResult(Validate(model, propertyName));

    public IEnumerable<string> Validate(object? model, string propertyName)
    {
        IEnumerable<string> errors = [];
        if (model != null)
        {
            if (model is IValidateable validateable)
            {
                var validationResult = validateable.Validate(propertyName);
                return TransformValidationResult(validationResult);
            }
            else
            {
                throw new NotSupportedException($"Model was not of supposed type '{nameof(IValidateable)}'");
            }
        }
        return errors;
    }

    public IEnumerable<string> Validate<T>(Expression<Func<T, object?>> propertyExpression, object? value) where T : class, IValidateable, new()
    {
        var body = propertyExpression.Body;
        while (body is UnaryExpression
            {
                NodeType: ExpressionType.Convert or ExpressionType.ConvertChecked
            } unary)
        {
            body = unary.Operand;
        }

        if (body is not MemberExpression
            {
                Member: PropertyInfo property,
                Expression: ParameterExpression
            })
        {
            throw new ArgumentException("The provided nested property expression does not reference a member!");
        }

        var instance = new T();
        property.SetValue(instance, value);

        var validationResult = instance.Validate(property.Name);
        return TransformValidationResult(validationResult);
    }

    IEnumerable<string> TransformValidationResult(ValidationResult validationResult)
    {
        IEnumerable<string> errors = [];
        switch (validationResult?.Status)
        {
            case ValidationStatus.NoValidation:
            case ValidationStatus.Success:
                // Nothing to do
                break;

            case ValidationStatus.Error:
                errors = [.. validationResult.ValidationMessages.Select(x => x.GetMessageLocalized(_localizer))];
                break;

            default:
                throw new InvalidOperationException("Unknown validation status.");
        }
        return errors;
    }
}
