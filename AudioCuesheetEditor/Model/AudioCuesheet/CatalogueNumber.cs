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
using AudioCuesheetEditor.Model.Entity;
using AudioCuesheetEditor.Model.Reflection;
using AudioCuesheetEditor.Model.UI;
using Blazorise.Localization;
using System;
using System.Collections.Generic;
using System.Linq;

namespace AudioCuesheetEditor.Model.AudioCuesheet
{
    public class Cataloguenumber : Validateable, IEntityDisplayName, ITraceable
    {
        public Cataloguenumber()
        {
            Validate();
        }

        private String? value;

        public event EventHandler<TraceablePropertiesChangedEventArgs>? TraceablePropertyChanged;

        public String? Value 
        {
            get => value;
            set 
            {
                var oldValue = this.value;
                this.value = value;
                FireEvents(oldValue, propertyName: nameof(Value));
            }
        }

        public String GetDisplayNameLocalized(ITextLocalizer localizer)
        {
            return localizer[nameof(Cuesheet)];
        }

        protected override void Validate()
        {
            if (String.IsNullOrEmpty(Value))
            {
                validationErrors.Add(new ValidationError(FieldReference.Create(this, nameof(Value)), ValidationErrorType.Warning, "{0} has no value!", nameof(Cataloguenumber)));
            }
            else
            {
                if (Value.All(Char.IsDigit) == false)
                {
                    validationErrors.Add(new ValidationError(FieldReference.Create(this, nameof(Value)), ValidationErrorType.Error, "{0} does not only contain numbers.", nameof(Cataloguenumber)));
                }
                if (Value.Length != 13)
                {
                    validationErrors.Add(new ValidationError(FieldReference.Create(this, nameof(Value)), ValidationErrorType.Error, "{0} has invalid length ({1})!", nameof(Cataloguenumber), 13));
                }
            }
        }

        private void OnTraceablePropertyChanged(object? previousValue, [System.Runtime.CompilerServices.CallerMemberName] string propertyName = "")
        {
            TraceablePropertyChanged?.Invoke(this, new TraceablePropertiesChangedEventArgs(new TraceableChange(previousValue, propertyName)));
        }

        /// <summary>
        /// Method for checking if fire of events should be done
        /// </summary>
        /// <param name="previousValue">Previous value of the property firing events</param>
        /// <param name="fireValidateablePropertyChanged">Fire ValidateablePropertyChanged?</param>
        /// <param name="fireTraceablePropertyChanged">Fire TraceablePropertyChanged?</param>
        /// <param name="propertyName">Property firing the event</param>
        /// <exception cref="NullReferenceException">If propertyName can not be found, an exception is thrown.</exception>
        private void FireEvents(object? previousValue, Boolean fireValidateablePropertyChanged = true, Boolean fireTraceablePropertyChanged = true, [System.Runtime.CompilerServices.CallerMemberName] string propertyName = "")
        {
            var propertyInfo = GetType().GetProperty(propertyName);
            if (propertyInfo != null)
            {
                var propertyValue = propertyInfo.GetValue(this);
                if (Equals(propertyValue, previousValue) == false)
                {
                    if (fireValidateablePropertyChanged)
                    {
                        OnValidateablePropertyChanged();
                    }
                    if (fireTraceablePropertyChanged)
                    {
                        OnTraceablePropertyChanged(previousValue, propertyName);
                    }
                }
            }
            else
            {
                throw new NullReferenceException(String.Format("Property {0} could not be found!", propertyName));
            }
        }
    }
}
