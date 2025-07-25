﻿//This file is part of AudioCuesheetEditor.

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

namespace AudioCuesheetEditor.Model.Options
{
    public class DownloadOptions : Validateable, IOptions
    {
        private string? projectFilename = Projectfile.DefaultFilename;
        private string? cuesheetFilename = Exportfile.DefaultCuesheetFilename;
        public String? CuesheetFilename
        {
            get => cuesheetFilename;
            set
            {
                if (String.IsNullOrEmpty(value) == false)
                {
                    var extension = Path.GetExtension(value);
                    if (extension?.Equals(FileExtensions.Cuesheet, StringComparison.OrdinalIgnoreCase) == false)
                    {
                        value = $"{value}{FileExtensions.Cuesheet}";
                    }
                }
                cuesheetFilename = value;
            }
        }
        public String? ProjectFilename
        {
            get => projectFilename;
            set
            {
                if (String.IsNullOrEmpty(value) == false)
                {
                    var extension = Path.GetExtension(value);
                    if (extension?.Equals(FileExtensions.Projectfile, StringComparison.OrdinalIgnoreCase) == false)
                    {
                        value = $"{value}{FileExtensions.Projectfile}";
                    }
                }
                projectFilename = value;
            }
        }
        public override ValidationResult Validate(string property)
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
