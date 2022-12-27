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
using AudioCuesheetEditor.Model.AudioCuesheet;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;

namespace AudioCuesheetEditor.Model.Entity
{
    public interface IValidateable
    {
        /// <summary>
        /// Validate all properties and return the result of validation.
        /// </summary>
        /// <returns>Validation result.</returns>
        ValidationResult Validate();
    }
    public interface IValidateable<T> : IValidateable
    {
        /// <summary>
        /// Validate a property and return the result of validation.
        /// </summary>
        /// <typeparam name="TProperty">Property type</typeparam>
        /// <param name="expression">Property selector</param>
        /// <returns>Validation result.</returns>
        ValidationResult Validate<TProperty>(Expression<Func<T, TProperty>> expression);
    }

    public enum ValidationStatus
    {
        NoValidation,
        Success,
        Error
    }
    public class ValidationResult
    {
        private List<String>? errors;

        public ValidationStatus Status { get; set; }
        public List<String>? ErrorMessages 
        {
            get => errors;
            set
            {
                errors = value;
                if ((errors != null) && errors.Any())
                {
                    Status = ValidationStatus.Error;
                }
            }
        }
    }
}
