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
using AudioCuesheetEditor.Model.IO.Export;
using AudioCuesheetEditor.Model.IO.Import;
using System.Text.Json.Serialization;

namespace AudioCuesheetEditor.Model.Options
{
    public class ExportOptions : IOptions
    {
        public static readonly Exportprofile DefaultSelectedExportProfile = new() 
        {
            Filename = "YouTube.txt",
            Name = "YouTube",
            SchemeHead = "%Cuesheet.Artist% - %Cuesheet.Title%",
            SchemeTracks = "%Track.Artist% - %Track.Title% %Track.Begin%"
        };
        public static readonly ICollection<Exportprofile> DefaultExportProfiles =
        [
            DefaultSelectedExportProfile,
            new Exportprofile()
            {
                Filename = "Mixcloud.txt",
                Name = "Mixcloud",
                SchemeTracks = "%Track.Artist% - %Track.Title% %Track.Begin%"
            },
            new Exportprofile()
            {
                Filename = "Export.csv",
                Name = "CSV Export",
                SchemeHead = "%Cuesheet.Artist%;%Cuesheet.Title%;",
                SchemeTracks = "%Track.Position%;%Track.Artist%;%Track.Title%;%Track.Begin%;%Track.End%;%Track.Length%",
                SchemeFooter = "Exported at %DateTime% using AudioCuesheetEditor (https://neocodermatrix86.github.io/AudioCuesheetEditor/)"
            },
            new Exportprofile()
            {
                Filename = "Tracks.txt",
                Name = "Tracks only",
                SchemeTracks = "%Track.Position% - %Track.Artist% - %Track.Title% - %Track.Begin% - %Track.End% - %Track.Length%",
            }
        ];
        public ExportOptions() 
        {
            ExportProfiles = DefaultExportProfiles;
            SelectedProfileId = DefaultSelectedExportProfile.Id;
        }
        [JsonConstructor]
        public ExportOptions(ICollection<Exportprofile> exportProfiles, Guid? selectedProfileId = null)
        {
            ExportProfiles = exportProfiles;
            SelectedProfileId = selectedProfileId ?? ExportProfiles.First().Id;
        }
        public ICollection<Exportprofile> ExportProfiles { get; set; }
        [JsonIgnore]
        public Exportprofile? SelectedExportProfile
        {
            get => SelectedProfileId.HasValue ? ExportProfiles.FirstOrDefault(x => x.Id == SelectedProfileId) : null;
            set
            {
                if (ExportProfiles.Any(x => x.Id == value?.Id) == false)
                {
                    if (value != null)
                    {
                        ExportProfiles.Add(value);
                    }
                }
                SelectedProfileId = value?.Id;
            }
        }
        public Guid? SelectedProfileId { get; private set; }
    }
}
