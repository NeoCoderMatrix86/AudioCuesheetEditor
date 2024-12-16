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
using AudioCuesheetEditor.Model.IO;
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
                    new("en-US"),
                    new("de-DE")
                };
                return cultures.AsReadOnly();
            }
        }
        public String? CuesheetFilename { get; set; } = Exportfile.DefaultCuesheetFilename;
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
        public Boolean LinkTracksWithPreviousOne { get; set; } = true;
        public String? ProjectFilename { get; set; } = Projectfile.DefaultFilename;
        public TimeSpanFormat? TimeSpanFormat { get; set; }
        public Boolean TracksTableSelectionVisible { get; set; } = false;
        public Boolean TracksTableHeaderPinned { get; set; } = false;

        protected override ValidationResult Validate(string property)
        {
            ValidationStatus validationStatus = ValidationStatus.NoValidation;
            List<ValidationMessage>? validationMessages = null;
            switch (property)
            {
                case nameof(CuesheetFilename):
                    validationStatus = ValidationStatus.Success;
                    if (string.IsNullOrEmpty(CuesheetFilename))
                    {
                        validationMessages ??= [];
                        validationMessages.Add(new ValidationMessage("{0} has no value!", nameof(CuesheetFilename)));
                    }
                    else
                    {
                        var extension = Path.GetExtension(CuesheetFilename);
                        if (extension.Equals(FileExtensions.Cuesheet, StringComparison.OrdinalIgnoreCase) == false)
                        {
                            validationMessages ??= [];
                            validationMessages.Add(new ValidationMessage("{0} must end with '{1}'!", nameof(CuesheetFilename), FileExtensions.Cuesheet));
                        }
                        var filenameWithoutExtension = Path.GetFileNameWithoutExtension(CuesheetFilename);
                        if (string.IsNullOrEmpty(filenameWithoutExtension))
                        {
                            validationMessages ??= [];
                            validationMessages.Add(new ValidationMessage("{0} must have a filename!", nameof(CuesheetFilename)));
                        }
                    }
                    break;
                case nameof(ProjectFilename):
                    validationStatus = ValidationStatus.Success;
                    if (String.IsNullOrEmpty(ProjectFilename))
                    {
                        validationMessages ??= [];
                        validationMessages.Add(new ValidationMessage("{0} has no value!", nameof(ProjectFilename)));
                    }
                    else
                    {
                        var extension = Path.GetExtension(ProjectFilename);
                        if (extension.Equals(FileExtensions.Projectfile, StringComparison.OrdinalIgnoreCase) == false)
                        {
                            validationMessages ??= [];
                            validationMessages.Add(new ValidationMessage("{0} must end with '{1}'!", nameof(ProjectFilename), FileExtensions.Projectfile));
                        }
                        var filename = Path.GetFileNameWithoutExtension(ProjectFilename);
                        if (String.IsNullOrEmpty(filename))
                        {
                            validationMessages ??= [];
                            validationMessages.Add(new ValidationMessage("{0} must have a filename!", nameof(ProjectFilename)));
                        }
                    }
                    break;
            }
            return ValidationResult.Create(validationStatus, validationMessages);
        }
    }
}
