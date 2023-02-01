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
using AudioCuesheetEditor.Model.Entity;

namespace AudioCuesheetEditor.Model.IO.Export
{
    public class SplitPoint : Validateable<SplitPoint>
    {
        //TODO: more details like artist, title and audiofile
        private TimeSpan? moment;

        public SplitPoint(Cuesheet cuesheet)
        {
            Cuesheet = cuesheet;
        }

        public Cuesheet Cuesheet { get; }

        public TimeSpan? Moment 
        { 
            get => moment;
            set
            {
                moment = value;
                OnValidateablePropertyChanged(nameof(Moment));
            }
        }

        protected override ValidationResult Validate(string property)
        {
            ValidationStatus validationStatus = ValidationStatus.NoValidation;
            List<ValidationMessage>? validationMessages = null;
            switch (property)
            {
                case nameof(Moment):
                    validationStatus = ValidationStatus.Success;
                    if (Moment == null)
                    {
                        validationMessages ??= new();
                        validationMessages.Add(new ValidationMessage("{0} has no value!", nameof(Moment)));
                    }
                    else
                    {
                        var maxEnd = Cuesheet.Tracks.Max(x => x.End);
                        if (Moment > maxEnd)
                        {
                            validationMessages ??= new();
                            validationMessages.Add(new ValidationMessage("{0} should be equal or less to '{1}'!", nameof(Moment), maxEnd));
                        }
                    }
                    break;
            }
            return ValidationResult.Create(validationStatus, validationMessages);
        }
    }
}
