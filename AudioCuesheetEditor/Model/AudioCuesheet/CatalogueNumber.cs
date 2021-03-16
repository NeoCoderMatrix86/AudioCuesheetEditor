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
using AudioCuesheetEditor.Shared.ResourceFiles;
using Blazorise.Localization;
using Microsoft.Extensions.Localization;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace AudioCuesheetEditor.Model.AudioCuesheet
{
    //TODO: Rename to Cataloguenumber
    public class CatalogueNumber : Validateable, IEntityDisplayName
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
                validationErrors.Add(new ValidationError(FieldReference.Create(this, nameof(Value)), ValidationErrorType.Warning, "{0} has no value!", nameof(CatalogueNumber)));
            }
            else
            {
                if (Value.All(Char.IsDigit) == false)
                {
                    validationErrors.Add(new ValidationError(FieldReference.Create(this, nameof(Value)), ValidationErrorType.Error, "{0} does not only contain numbers.", nameof(CatalogueNumber)));
                }
                if (Value.Length != 13)
                {
                    validationErrors.Add(new ValidationError(FieldReference.Create(this, nameof(Value)), ValidationErrorType.Error, "{0} has invalid length ({1})!", nameof(CatalogueNumber), 13));
                }
            }
        }

        public String GetDisplayNameLocalized(ITextLocalizer localizer)
        {
            return localizer[nameof(Cuesheet)];
        }
    }
}
