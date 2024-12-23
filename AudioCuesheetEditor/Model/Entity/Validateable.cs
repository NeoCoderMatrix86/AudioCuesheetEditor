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
namespace AudioCuesheetEditor.Model.Entity
{
    public abstract class Validateable : IValidateable
    {
        public event EventHandler<String>? ValidateablePropertyChanged;

        //TODO
        //public ValidationResult Validate<TProperty>(Expression<Func<T, TProperty>> expression)
        //{
        //    if (expression.Body is MemberExpression memberExpression)
        //    {
        //        return Validate(memberExpression.Member.Name);
        //    }
        //    else if (expression.Body is UnaryExpression unaryExpression && unaryExpression.Operand is MemberExpression unaryMemberExpression)
        //    {
        //        return Validate(unaryMemberExpression.Member.Name);
        //    }
        //    else
        //    {
        //        throw new ArgumentException("The provided expression does not reference a valid property.");
        //    }
        //}

        public ValidationResult Validate()
        {
            ValidationResult validationResult = new() { Status = ValidationStatus.NoValidation };
            List<ValidationMessage> validationMessages = [];
            //TODO
            //foreach (var property in typeof(T).GetProperties(BindingFlags.Instance | BindingFlags.Public))
            //{
            //    var result = Validate(property.Name);
            //    validationMessages.AddRange(result.ValidationMessages);
            //    switch (validationResult.Status)
            //    {
            //        case ValidationStatus.NoValidation:
            //        case ValidationStatus.Success:
            //            switch (result.Status)
            //            {
            //                case ValidationStatus.Success:
            //                case ValidationStatus.Error:
            //                    validationResult.Status = result.Status;
            //                    break;
            //            }
            //            break;
            //        case ValidationStatus.Error:
            //            //If there was an error, we don't delete it!
            //            break;
            //    }
            //}
            validationResult.ValidationMessages = validationMessages;
            return validationResult;
        }

        public abstract ValidationResult Validate(String property);

        protected void OnValidateablePropertyChanged([System.Runtime.CompilerServices.CallerMemberName] string propertyName = "")
        {
            ValidateablePropertyChanged?.Invoke(this, propertyName);
        }
    }
}
