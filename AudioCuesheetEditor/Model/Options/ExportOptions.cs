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
        public IReadOnlyCollection<Exportprofile> ExportProfiles { get; set; }

#pragma warning disable CS8618 // Ein Non-Nullable-Feld muss beim Beenden des Konstruktors einen Wert ungleich NULL enthalten. Erwägen Sie die Deklaration als Nullable.
        public ExportOptions()
#pragma warning restore CS8618 // Ein Non-Nullable-Feld muss beim Beenden des Konstruktors einen Wert ungleich NULL enthalten. Erwägen Sie die Deklaration als Nullable.
        {
            SetDefaultValues();
        }

        public void SetDefaultValues()
        {
            //Declare defaults
            if (ExportProfiles == null)
            {
                var list = new List<Exportprofile>();
                var exportProfile = new Exportprofile()
                {
                    FileName = "YouTube.txt",
                    Name = "YouTube"
                };
                exportProfile.SchemeHead.Scheme = "%Cuesheet.Artist% - %Cuesheet.Title%";
                exportProfile.SchemeTracks.Scheme = "%Track.Artist% - %Track.Title% %Track.Begin%";
                exportProfile.SchemeFooter.Scheme = String.Empty;
                list.Add(exportProfile);
                exportProfile = new Exportprofile()
                {
                    FileName = "Mixcloud.txt",
                    Name = "Mixcloud"
                };
                exportProfile.SchemeHead.Scheme = String.Empty;
                exportProfile.SchemeTracks.Scheme = "%Track.Artist% - %Track.Title% %Track.Begin%";
                exportProfile.SchemeFooter.Scheme = String.Empty;
                list.Add(exportProfile);
                exportProfile = new Exportprofile()
                {
                    FileName = "Export.csv",
                    Name = "CSV Export"
                };
                exportProfile.SchemeHead.Scheme = "%Cuesheet.Artist%;%Cuesheet.Title%;";
                exportProfile.SchemeTracks.Scheme = "%Track.Position%;%Track.Artist%;%Track.Title%;%Track.Begin%;%Track.End%;%Track.Length%";
                exportProfile.SchemeFooter.Scheme = "Exported at %DateTime% using AudioCuesheetEditor (https://neocodermatrix86.github.io/AudioCuesheetEditor/)";
                list.Add(exportProfile);
                exportProfile = new Exportprofile()
                {
                    FileName = "Tracks.txt",
                    Name = "Tracks only"
                };
                exportProfile.SchemeHead.Scheme = String.Empty;
                exportProfile.SchemeTracks.Scheme = "%Track.Position% - %Track.Artist% - %Track.Title% - %Track.Begin% - %Track.End% - %Track.Length%";
                exportProfile.SchemeFooter.Scheme = String.Empty;
                list.Add(exportProfile);
                ExportProfiles = list.AsReadOnly();
            }
        }
    }
}
