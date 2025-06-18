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
using AudioCuesheetEditor.Model.AudioCuesheet.Import;
using AudioCuesheetEditor.Model.Entity;
using AudioCuesheetEditor.Model.IO;
using AudioCuesheetEditor.Model.IO.Export;
using AudioCuesheetEditor.Model.IO.Import;
using AudioCuesheetEditor.Model.Utility;
using AudioCuesheetEditor.Services.UI;
using System.Globalization;
using System.Text.Json.Serialization;

namespace AudioCuesheetEditor.Model.Options
{
    public enum ViewMode
    {
        DetailView = 0,
        RecordView = 1,
        ImportView = 2
    }
    public class ApplicationOptions : Validateable, IOptions
    {
        public const LogLevel DefaultLogLevel = LogLevel.Information;
        public static readonly Importprofile DefaultSelectedImportprofile = new()
        {
            Name = "Textfile (common data in first line)",
            UseRegularExpression = false,
            SchemeCuesheet = $"{nameof(ImportCuesheet.Artist)} - {nameof(ImportCuesheet.Title)}\t{nameof(ImportCuesheet.Audiofile)}",
            SchemeTracks = $"{nameof(ImportTrack.Artist)} - {nameof(ImportTrack.Title)}\t{nameof(ImportTrack.End)}"
        };
        public static readonly ICollection<Importprofile> DefaultImportprofiles =
        [
            DefaultSelectedImportprofile,
            new()
            {
                Name = "Textfile (just track data)",
                UseRegularExpression = false,
                SchemeTracks = $"{nameof(ImportTrack.Artist)} - {nameof(ImportTrack.Title)}\t{nameof(ImportTrack.End)}"
            },
            new()
            {
                Name = "Textfile (track data seperated by ~)",
                UseRegularExpression = false,
                SchemeTracks = $"{nameof(ImportTrack.Artist)}~{nameof(ImportTrack.Title)}~{nameof(ImportTrack.StartDateTime)}"
            },
            new()
            {
                Name = "Traktor history",
                UseRegularExpression = true,
                SchemeTracks = @$"<tr>\s*<td>(?<{nameof(ImportTrack.Position)}>\d+)</td>\s*<td>(?<{nameof(ImportTrack.Artist)}>.*?)</td>\s*<td>(?<{nameof(ImportTrack.Title)}>.*?)</td>\s*<td>(?<{nameof(ImportTrack.StartDateTime)}>.*?)</td>\s*</tr>"
            }
        ];
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
        public String? CultureName { get; set; } = LocalizationService.DefaultCulture;
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
        public ViewMode ActiveTab { get; set; }
        public String? ActiveTabName
        {
            get => Enum.GetName(ActiveTab);
            set
            {
                if (value != null)
                {
                    ActiveTab = Enum.Parse<ViewMode>(value);
                }
                else
                {
                    throw new ArgumentNullException(nameof(value));
                }
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
        public TimeSpanFormat? TimeSpanFormat { get; set; }
        public Boolean LinkTracks { get; set; } = true;
        [Obsolete("Will be deleted!")]
        public TextImportScheme ImportScheme { get; set; } = TextImportScheme.DefaultTextImportScheme;
        [Obsolete("Will be deleted!")]
        public TimeSpanFormat ImportTimeSpanFormat { get; set; } = new();
        public uint RecordCountdownTimer { get; set; } = 5;
        public Boolean FixedTracksTableHeader { get; set; } = false;
        public String? DisplayTimeSpanFormat { get; set; }
        public LogLevel MinimumLogLevel { get; set; } = DefaultLogLevel;
        [JsonInclude]
        public Guid? SelectedImportProfileId { get; private set; }
        public ICollection<Importprofile> ImportProfiles { get; set; } = DefaultImportprofiles;
        [JsonIgnore]
        public Importprofile? SelectedImportProfile
        {
            get => SelectedImportProfileId.HasValue ? ImportProfiles.FirstOrDefault(x => x.Id == SelectedImportProfileId) : null;
            set
            {
                if (ImportProfiles.Any(x => x.Id == value?.Id) == false)
                {
                    if (value != null)
                    {
                        ImportProfiles.Add(value);
                    }
                }
                SelectedImportProfileId = value?.Id;
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
