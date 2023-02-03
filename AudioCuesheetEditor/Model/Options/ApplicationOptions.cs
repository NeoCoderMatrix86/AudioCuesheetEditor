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
using AudioCuesheetEditor.Model.IO;
using AudioCuesheetEditor.Model.IO.Audio;
using AudioCuesheetEditor.Model.IO.Export;
using AudioCuesheetEditor.Model.Utility;
using System.Globalization;
using System.Text.Json.Serialization;

namespace AudioCuesheetEditor.Model.Options
{
    /// <summary>
    /// Enum for setting desired GUI mode
    /// </summary>
    public enum ViewMode
    {
        ViewModeFull = 0,
        ViewModeRecord = 1,
        ViewModeImport = 2
    }

    public enum TimeSensitivityMode
    {
        Full = 0,
        Seconds = 1,
        Minutes = 2
    }

    public class ApplicationOptions : Validateable<ApplicationOptions>, IOptions
    {
        public const String DefaultCultureName = "en-US";

        public static IReadOnlyCollection<CultureInfo> AvailableCultures
        {
            get
            {
                var cultures = new List<CultureInfo>
                {
                    new CultureInfo("en-US"),
                    new CultureInfo("de-DE")
                };
                return cultures.AsReadOnly();
            }
        }
        public String? CuesheetFilename { get; set; } = Cuesheetfile.DefaultFilename;
        public String? CultureName { get; set; } = DefaultCultureName;
        [JsonIgnore]
        public CultureInfo Culture
        {
            get
            {
                if (String.IsNullOrEmpty(CultureName) == false)
                {
                    return new CultureInfo(CultureName);
                }
                else
                {
                    return CultureInfo.CurrentCulture;
                }
            }
        }
        [JsonIgnore]
        public ViewMode ViewMode { get; set; }
        public String? ViewModename 
        {
            get { return Enum.GetName(typeof(ViewMode), ViewMode); }
            set 
            {
                if (value != null)
                {
                    ViewMode = (ViewMode)Enum.Parse(typeof(ViewMode), value);
                }
                else
                {
                    throw new ArgumentNullException(nameof(value));
                }
            }
        }
        public String RecordedAudiofilename { get; set; } = Audiofile.RecordingFileName;
        public Boolean LinkTracksWithPreviousOne { get; set; } = true;
        public String? ProjectFilename { get; set; } = Projectfile.DefaultFilename;
        public uint RecordCountdownTimer { get; set; } = 5;
        [JsonIgnore]
        public TimeSensitivityMode RecordTimeSensitivity { get; set; }
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
        public TimeSpanFormat? TimeSpanFormat { get; set; }

        protected override ValidationResult Validate(string property)
        {
            ValidationStatus validationStatus = ValidationStatus.NoValidation;
            List<ValidationMessage>? validationMessages = null;
            switch (property)
            {
                case nameof(CuesheetFilename):
                    return Cuesheetfile.ValidateFilename(CuesheetFilename);
                case nameof(RecordedAudiofilename):
                    validationStatus = ValidationStatus.Success;
                    if (String.IsNullOrEmpty(RecordedAudiofilename))
                    {
                        validationMessages ??= new();
                        validationMessages.Add(new ValidationMessage("{0} has no value!", nameof(RecordedAudiofilename)));
                    }
                    else
                    {
                        var extension = Path.GetExtension(RecordedAudiofilename);
                        if (extension.Equals(Audiofile.AudioCodecWEBM.FileExtension, StringComparison.OrdinalIgnoreCase) == false)
                        {
                            validationMessages ??= new();
                            validationMessages.Add(new ValidationMessage("{0} must end with '{1}'!", nameof(RecordedAudiofilename), Audiofile.AudioCodecWEBM.FileExtension));
                        }
                        var filename = Path.GetFileNameWithoutExtension(RecordedAudiofilename);
                        if (String.IsNullOrEmpty(filename))
                        {
                            validationMessages ??= new();
                            validationMessages.Add(new ValidationMessage("{0} must have a filename!", nameof(RecordedAudiofilename)));
                        }
                    }
                    break;
                case nameof(ProjectFilename):
                    validationStatus = ValidationStatus.Success;
                    if (String.IsNullOrEmpty(ProjectFilename))
                    {
                        validationMessages ??= new();
                        validationMessages.Add(new ValidationMessage("{0} has no value!", nameof(ProjectFilename)));
                    }
                    else
                    {
                        var extension = Path.GetExtension(ProjectFilename);
                        if (extension.Equals(Projectfile.FileExtension, StringComparison.OrdinalIgnoreCase) == false)
                        {
                            validationMessages ??= new();
                            validationMessages.Add(new ValidationMessage("{0} must end with '{1}'!", nameof(ProjectFilename), Projectfile.FileExtension));
                        }
                        var filename = Path.GetFileNameWithoutExtension(ProjectFilename);
                        if (String.IsNullOrEmpty(filename))
                        {
                            validationMessages ??= new();
                            validationMessages.Add(new ValidationMessage("{0} must have a filename!", nameof(ProjectFilename)));
                        }
                    }
                    break;
            }
            return ValidationResult.Create(validationStatus, validationMessages);
        }
    }
}
