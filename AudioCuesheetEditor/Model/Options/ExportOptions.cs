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

namespace AudioCuesheetEditor.Model.Options
{
    public class ExportOptions : IOptions
    {
        public static readonly ICollection<Exportprofile> DefaultExportProfiles =
        [
            new Exportprofile()
            {
                Id = Guid.NewGuid(),
                Filename = "YouTube.txt",
                Name = "YouTube",
                SchemeHead = "%Cuesheet.Artist% - %Cuesheet.Title%",
                SchemeTracks = "%Track.Artist% - %Track.Title% %Track.Begin%"
            },
            new Exportprofile()
            {
                Id = Guid.NewGuid(),
                Filename = "Mixcloud.txt",
                Name = "Mixcloud",
                SchemeTracks = "%Track.Artist% - %Track.Title% %Track.Begin%"
            },
            new Exportprofile()
            {
                Id = Guid.NewGuid(),
                Filename = "Export.csv",
                Name = "CSV Export",
                SchemeHead = "%Cuesheet.Artist%;%Cuesheet.Title%;",
                SchemeTracks = "%Track.Position%;%Track.Artist%;%Track.Title%;%Track.Begin%;%Track.End%;%Track.Length%",
                SchemeFooter = "Exported at %DateTime% using AudioCuesheetEditor (https://neocodermatrix86.github.io/AudioCuesheetEditor/)"
            },
            new Exportprofile()
            {
                Id = Guid.NewGuid(),
                Filename = "Tracks.txt",
                Name = "Tracks only",
                SchemeTracks = "%Track.Position% - %Track.Artist% - %Track.Title% - %Track.Begin% - %Track.End% - %Track.Length%",
            }
        ];
        public ICollection<Exportprofile> ExportProfiles { get; set; } = DefaultExportProfiles;
        //TODO: Save selected profile
        //[JsonIgnore]
        //public Exportprofile SelectedExportProfile
        //{
        //    get => ExportProfiles.First(x => x.Id == SelectedProfileId);
        //    set => SelectedProfileId = value.Id;
        //}
        //public Guid SelectedProfileId { get; private set; } = DefaultSelectedExportProfile.Id;
    }
}
