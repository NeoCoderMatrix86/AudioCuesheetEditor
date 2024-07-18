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

namespace AudioCuesheetEditor.Model.Options
{
    public class RecordOptions : Validateable<RecordOptions>, IOptions
    {
        public String RecordedAudiofilename { get; set; } = Audiofile.RecordingFileName;
        [JsonIgnore]
        public TimeSensitivityMode RecordTimeSensitivity { get; set; }
        public uint RecordCountdownTimer { get; set; } = 5;
        public String? RecordTimeSensitivityname
        {
            get { return Enum.GetName(typeof(TimeSensitivityMode), RecordTimeSensitivity); }
            set
            {
                if (value != null)
                {
                    RecordTimeSensitivity = (TimeSensitivityMode)Enum.Parse(typeof(TimeSensitivityMode), value);
                }
                else
                {
                    throw new ArgumentNullException(nameof(value));
                }
            }
        }
        protected override ValidationResult Validate(string property)
        {
            ValidationStatus validationStatus = ValidationStatus.NoValidation;
            List<ValidationMessage>? validationMessages = null;
            switch (property)
            {
                case nameof(RecordedAudiofilename):
                    validationStatus = ValidationStatus.Success;
                    if (String.IsNullOrEmpty(RecordedAudiofilename))
                    {
                        validationMessages ??= [];
                        validationMessages.Add(new ValidationMessage("{0} has no value!", nameof(RecordedAudiofilename)));
                    }
                    else
                    {
                        var extension = Path.GetExtension(RecordedAudiofilename);
                        if (extension.Equals(Audiofile.AudioCodecWEBM.FileExtension, StringComparison.OrdinalIgnoreCase) == false)
                        {
                            validationMessages ??= [];
                            validationMessages.Add(new ValidationMessage("{0} must end with '{1}'!", nameof(RecordedAudiofilename), Audiofile.AudioCodecWEBM.FileExtension));
                        }
                        var filename = Path.GetFileNameWithoutExtension(RecordedAudiofilename);
                        if (String.IsNullOrEmpty(filename))
                        {
                            validationMessages ??= [];
                            validationMessages.Add(new ValidationMessage("{0} must have a filename!", nameof(RecordedAudiofilename)));
                        }
                    }
                    break;
            }
            return ValidationResult.Create(validationStatus, validationMessages);
        }
    }
}
