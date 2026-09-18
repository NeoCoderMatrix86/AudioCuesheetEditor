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
using AudioCuesheetEditor.Model.IO.Audio;
using System.Text.Json.Serialization;

namespace AudioCuesheetEditor.Model.AudioCuesheet
{
    public class Cuesheet() : Validateable, ICuesheet
    {
        public String? Artist { get; set; }
        
        public String? Title { get; set; }

        public IList<Audiofile> Audiofiles { get; set; } = [];

        public CDTextfile? CDTextfile { get; set; }

        public String? Cataloguenumber { get; set; }

        [JsonIgnore]
        public bool IsRecording => RecordingStart.HasValue;
        
        [JsonIgnore]
        public DateTime? RecordingStart { get; set; }

        public override ValidationResult Validate(string property)
        {
            ValidationStatus validationStatus = ValidationStatus.NoValidation;
            List<ValidationMessage>? validationMessages = null;
            switch (property)
            {
                case nameof(Audiofiles):
                    validationStatus = ValidationStatus.Success;
                    if (Audiofiles.Count == 0)
                    {
                        validationMessages ??= [];
                        validationMessages.Add(new ValidationMessage("{0} has invalid count ({1})!", nameof(Audiofiles), 0));
                    }
                    break;
                case nameof(Artist):
                    validationStatus = ValidationStatus.Success;
                    if (String.IsNullOrEmpty(Artist))
                    {
                        validationMessages ??= [];
                        validationMessages.Add(new ValidationMessage("{0} has no value!", nameof(Artist)));
                    }
                    break;
                case nameof(Title):
                    validationStatus = ValidationStatus.Success;
                    if (String.IsNullOrEmpty(Title))
                    {
                        validationMessages ??= [];
                        validationMessages.Add(new ValidationMessage("{0} has no value!", nameof(Title)));
                    }
                    break;
                case nameof(Cataloguenumber):
                    validationStatus = ValidationStatus.Success;
                    if (String.IsNullOrEmpty(Cataloguenumber) == false)
                    {
                        if (Cataloguenumber.All(Char.IsDigit) == false)
                        {
                            validationMessages ??= [];
                            validationMessages.Add(new ValidationMessage("{0} must only contain numbers!", nameof(Cataloguenumber)));
                        }
                        if (Cataloguenumber.Length != 13)
                        {
                            validationMessages ??= [];
                            validationMessages.Add(new ValidationMessage("{0} has an invalid length. Allowed length is {1}!", nameof(Cataloguenumber), 13));
                        }
                    }
                    break;
            }
            return ValidationResult.Create(validationStatus, validationMessages);
        }
    }
}
