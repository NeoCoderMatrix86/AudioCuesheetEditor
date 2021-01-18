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
using AudioCuesheetEditor.Controller;
using AudioCuesheetEditor.Model.Entity;
using AudioCuesheetEditor.Model.Reflection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace AudioCuesheetEditor.Model.AudioCuesheet
{
    public class CatalogueNumber : Validateable
    {
        public CatalogueNumber()
        {
            Validate();
        }

        private String value;
        public String Value 
        {
            get { return value; }
            set { this.value = value; OnValidateablePropertyChanged(); }
        }
        protected override void Validate()
        {
            if (String.IsNullOrEmpty(Value))
            {
                validationErrors.Add(new ValidationError(FieldReference.Create(this, nameof(Value)), ValidationErrorType.Warning, "HasNoValue", "CatalogueNumber"));
            }
            else
            {
                if (Value.All(Char.IsDigit) == false)
                {
                    validationErrors.Add(new ValidationError(FieldReference.Create(this, nameof(Value)), ValidationErrorType.Error, "CataloguenumberContainsNonDigits"));
                }
                if (Value.Length != 13)
                {
                    validationErrors.Add(new ValidationError(FieldReference.Create(this, nameof(Value)), ValidationErrorType.Error, "HasInvalidLength", "CatalogueNumber", 13));
                }
            }
        }
    }
}
