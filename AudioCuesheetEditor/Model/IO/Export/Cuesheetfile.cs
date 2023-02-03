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
using AudioCuesheetEditor.Model.Options;
using Microsoft.AspNetCore.Components.Forms;

namespace AudioCuesheetEditor.Model.IO.Export
{
    public class Cuesheetfile
    {
        public static readonly string DefaultFilename = "Cuesheet.cue";

        public static ValidationResult ValidateFilename(string? filename)
        {
            ValidationStatus validationStatus = ValidationStatus.Success;
            List<ValidationMessage>? validationMessages = null;
            if (string.IsNullOrEmpty(filename))
            {
                validationMessages ??= new();
                validationMessages.Add(new ValidationMessage("{0} has no value!", nameof(ApplicationOptions.CuesheetFilename)));
            }
            else
            {
                var extension = Path.GetExtension(filename);
                if (extension.Equals(Cuesheet.FileExtension, StringComparison.OrdinalIgnoreCase) == false)
                {
                    validationMessages ??= new();
                    validationMessages.Add(new ValidationMessage("{0} must end with '{1}'!", nameof(ApplicationOptions.CuesheetFilename), Cuesheet.FileExtension));
                }
                var filenameWithoutExtension = Path.GetFileNameWithoutExtension(filename);
                if (string.IsNullOrEmpty(filenameWithoutExtension))
                {
                    validationMessages ??= new();
                    validationMessages.Add(new ValidationMessage("{0} must have a filename!", nameof(ApplicationOptions.CuesheetFilename)));
                }
            }
            return ValidationResult.Create(validationStatus, validationMessages);
        }

        public string Filename { get; set; } = DefaultFilename;
        public byte[]? Content { get; set; }
        public TimeSpan? Begin { get; set; }
        public TimeSpan? End { get; set; }

    }
}
