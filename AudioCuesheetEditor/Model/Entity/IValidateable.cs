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
    public interface IValidateable
    {
        /// <summary>
        /// Validate all properties and return the result of validation.
        /// </summary>
        /// <returns>Validation result.</returns>
        ValidationResult Validate();
        /// <summary>
        /// Validates the property and return the result of validation.
        /// </summary>
        /// <param name="property"></param>
        /// <returns></returns>
        ValidationResult Validate(String property);

        event EventHandler<String>? ValidateablePropertyChanged;
    }
    public enum ValidationStatus
    {
        NoValidation,
        Success,
        Error
    }
    public class ValidationResult
    {
        private List<ValidationMessage> validationMessages = [];

        public static ValidationResult Create(ValidationStatus validationStatus, IEnumerable<ValidationMessage>? validationMessages = null)
        {
            var result = new ValidationResult() { Status = validationStatus };
            if (validationMessages != null) 
            {
                result.ValidationMessages = [.. validationMessages];
            }
            return result;
        }

        public ValidationStatus Status { get; set; }
        public ICollection<ValidationMessage> ValidationMessages 
        {
            get => validationMessages;
            set
            {
                validationMessages = [.. value];
                if (validationMessages.Count > 0)
                {
                    Status = ValidationStatus.Error;
                }
            }
        }
    }
}
