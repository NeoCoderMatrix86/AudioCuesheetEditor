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
        public static readonly List<Exportprofile> DefaultExportProfiles = new() 
        { 
            new Exportprofile()
            {
                Filename = "YouTube.txt",
                Name = "YouTube",
                SchemeHead = new Exportscheme() { Scheme = "%Cuesheet.Artist% - %Cuesheet.Title%" },
                SchemeTracks = new Exportscheme() { Scheme = "%Track.Artist% - %Track.Title% %Track.Begin%" }
            },
            new Exportprofile()
            {
                Filename = "Mixcloud.txt",
                Name = "Mixcloud",
                SchemeTracks = new Exportscheme() { Scheme = "%Track.Artist% - %Track.Title% %Track.Begin%" }
            },
            new Exportprofile()
            {
                Filename = "Export.csv",
                Name = "CSV Export",
                SchemeHead = new Exportscheme() { Scheme = "%Cuesheet.Artist%;%Cuesheet.Title%;" },
                SchemeTracks = new Exportscheme() { Scheme = "%Track.Position%;%Track.Artist%;%Track.Title%;%Track.Begin%;%Track.End%;%Track.Length%" },
                SchemeFooter = new Exportscheme() { Scheme = "Exported at %DateTime% using AudioCuesheetEditor (https://neocodermatrix86.github.io/AudioCuesheetEditor/)" }
            },
            new Exportprofile()
            {
                Filename = "Tracks.txt",
                Name = "Tracks only",
                SchemeTracks = new Exportscheme() { Scheme = "%Track.Position% - %Track.Artist% - %Track.Title% - %Track.Begin% - %Track.End% - %Track.Length%" },
            }
        };
        public IReadOnlyCollection<Exportprofile> ExportProfiles { get; set; } = DefaultExportProfiles;
    }
}
