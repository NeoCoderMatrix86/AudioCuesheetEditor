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
using AudioCuesheetEditor.Model.IO.Import;
using AudioCuesheetEditor.Model.Utility;
using AudioCuesheetEditor.Services.UI;
using System.Globalization;
using System.Text.Json.Serialization;

namespace AudioCuesheetEditor.Model.Options
{
    public class ApplicationOptions : IOptions
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
        public TimeSpanFormat? TimeSpanFormat { get; set; }
        public Boolean LinkTracks { get; set; } = true;
        public Boolean FixedTracksTableHeader { get; set; } = false;
        public String? DisplayTimeSpanFormat { get; set; }
        public LogLevel MinimumLogLevel { get; set; } = DefaultLogLevel;
        [JsonInclude]
        public Guid? SelectedImportProfileId { get; private set; } = DefaultSelectedImportprofile.Id;
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
    }
}
