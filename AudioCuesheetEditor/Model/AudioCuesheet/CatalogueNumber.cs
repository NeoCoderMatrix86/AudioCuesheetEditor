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
        private readonly CuesheetController _cuesheetController;

        public CatalogueNumber(CuesheetController cuesheetController)
        {
            _cuesheetController = cuesheetController;
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
                validationErrors.Add(new ValidationError(String.Format(_cuesheetController.GetLocalizedString("HasNoValue"), _cuesheetController.GetLocalizedString("CatalogueNumber")), FieldReference.Create(this, nameof(Value)), ValidationErrorType.Warning));
            }
            else
            {
                if (Value.All(Char.IsDigit) == false)
                {
                    validationErrors.Add(new ValidationError(_cuesheetController.GetLocalizedString("CataloguenumberContainsNonDigits"), FieldReference.Create(this, nameof(Value)), ValidationErrorType.Error));
                }
                if (Value.Length != 13)
                {
                    validationErrors.Add(new ValidationError(String.Format(_cuesheetController.GetLocalizedString("HasInvalidLength"), _cuesheetController.GetLocalizedString("CatalogueNumber"), 13), FieldReference.Create(this, nameof(Value)), ValidationErrorType.Error));
                }
            }
        }
    }
}
