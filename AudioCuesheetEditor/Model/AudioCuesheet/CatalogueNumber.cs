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
using AudioCuesheetEditor.Model.UI;

namespace AudioCuesheetEditor.Model.AudioCuesheet
{
    public class Cataloguenumber : Validateable<Cataloguenumber>, ITraceable
    {
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
        protected override ValidationResult Validate(string property)
        {
            var result = new ValidationResult() { Status = ValidationStatus.NoValidation };
            List<String>? errors = null;
            switch (property)
            {
                case nameof(Value):
                    if (String.IsNullOrEmpty(Value) == false)
                    {
                        if (Value.All(Char.IsDigit) == false)
                        {
                            errors = new() { String.Format("{0} must only contain numbers!", nameof(Value)) };
                        }
                        else
                        {
                            result.Status = ValidationStatus.Success;
                        }
                        if (Value.Length != 13)
                        {
                            errors ??= new();
                            errors.Add(String.Format("{0} has an invalid length. Allowed length is {1}!", nameof(Value), 13));
                        }
                        else
                        {
                            result.Status = ValidationStatus.Success;
                        }
                    }
                    break;
            }
            result.ErrorMessages = errors;
            return result;
        }

        private void OnTraceablePropertyChanged(object? previousValue, [System.Runtime.CompilerServices.CallerMemberName] string propertyName = "")
        {
            TraceablePropertyChanged?.Invoke(this, new TraceablePropertiesChangedEventArgs(new TraceableChange(previousValue, propertyName)));
        }

        /// <summary>
        /// Method for checking if fire of events should be done
        /// </summary>
        /// <param name="previousValue">Previous value of the property firing events</param>
        /// <param name="fireTraceablePropertyChanged">Fire TraceablePropertyChanged?</param>
        /// <param name="propertyName">Property firing the event</param>
        /// <exception cref="NullReferenceException">If propertyName can not be found, an exception is thrown.</exception>
        private void FireEvents(object? previousValue, Boolean fireTraceablePropertyChanged = true, [System.Runtime.CompilerServices.CallerMemberName] string propertyName = "")
        {
            var propertyInfo = GetType().GetProperty(propertyName);
            if (propertyInfo != null)
            {
                var propertyValue = propertyInfo.GetValue(this);
                if (Equals(propertyValue, previousValue) == false)
                {
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
