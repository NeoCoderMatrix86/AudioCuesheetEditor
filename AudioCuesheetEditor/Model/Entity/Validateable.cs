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
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace AudioCuesheetEditor.Model.Entity
{
    public abstract class Validateable : IValidateable
    {
        protected List<ValidationError> validationErrors = new List<ValidationError>();

        public bool IsValid
        {
            get { return validationErrors.Count == 0; }
        }

        public IReadOnlyCollection<ValidationError> ValidationErrors
        {
            get { return validationErrors.AsReadOnly(); }
        }

        public IReadOnlyCollection<ValidationError> GetValidationErrorsFiltered(String property = null, ValidationErrorFilterType validationErrorFilterType = ValidationErrorFilterType.All)
        {
            IReadOnlyCollection<ValidationError> returnValue = ValidationErrors;
            if (String.IsNullOrEmpty(property) == false)
            {
                returnValue = ValidationErrors.Where(x => x.FieldReference.DisplayName == property).ToList().AsReadOnly();
            }
            switch (validationErrorFilterType)
            {
                case ValidationErrorFilterType.ErrorOnly:
                    returnValue = returnValue.Where(x => x.Type == ValidationErrorType.Error).ToList().AsReadOnly();
                    break;
                case ValidationErrorFilterType.WarningOnly:
                    returnValue = returnValue.Where(x => x.Type == ValidationErrorType.Warning).ToList().AsReadOnly();
                    break;
                case ValidationErrorFilterType.All:
                default:
                    break;
            }
            return returnValue;
        }

        public String GetValidationErrors(String property = null, ValidationErrorFilterType validationErrorFilterType = ValidationErrorFilterType.All,  String seperator = "<br />")
        {
            var errorsFiltered = GetValidationErrorsFiltered(property, validationErrorFilterType);
            if (errorsFiltered.Any())
            {
                return String.Join(seperator, errorsFiltered.OrderBy(y => y.Type).Select(x => x.Message));
            }
            return null;
        }

        public event EventHandler ValidateablePropertyChanged;

        protected void OnValidateablePropertyChanged()
        {
            validationErrors.Clear();
            Validate();
            ValidateablePropertyChanged?.Invoke(this, EventArgs.Empty);
        }

        protected abstract void Validate();
    }
}
