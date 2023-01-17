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
using System;
using System.Text;
using System.Text.Json.Serialization;

namespace AudioCuesheetEditor.Model.IO.Export
{
    public class Exportprofile
    {
        public static readonly String DefaultFileName = "Export.txt";

        public Exportprofile()
        {
            SchemeHead = new Exportscheme
            {
                SchemeType = Schemetype.Header
            };
            SchemeTracks = new Exportscheme
            {
                SchemeType = Schemetype.Body
            };
            SchemeFooter = new Exportscheme
            {
                SchemeType = Schemetype.Footer
            };
            FileName = DefaultFileName;
            var random = new Random();
            Name = String.Format("{0}_{1}", nameof(Exportprofile), random.Next(1, 100));
        }
        public String Name { get; set; }
        public Exportscheme SchemeHead { get; set; }
        public Exportscheme SchemeTracks { get; set; }
        public Exportscheme SchemeFooter { get; set; }
        public String FileName { get; set; }
        [JsonIgnore]
        public Boolean IsExportable
        {
            get
            {
                return (SchemeHead.Validate().Status == Entity.ValidationStatus.Success)
                    && (SchemeTracks.Validate().Status == Entity.ValidationStatus.Success)
                    && (SchemeFooter.Validate().Status == Entity.ValidationStatus.Success);
                
            }
        }
        public byte[]? GenerateExport(Cuesheet cuesheet)
        {
            if (IsExportable == true)
            {
                var builder = new StringBuilder();
                if (SchemeHead != null)
                {
                    builder.AppendLine(SchemeHead.GetExportResult(cuesheet));
                }
                if (SchemeTracks != null)
                {
                    foreach (var track in cuesheet.Tracks)
                    {
                        builder.AppendLine(SchemeTracks.GetExportResult(track));
                    }
                }
                if (SchemeFooter != null)
                {
                    builder.AppendLine(SchemeFooter.GetExportResult(cuesheet));
                }
                return Encoding.UTF8.GetBytes(builder.ToString());
            }
            else
            {
                return null;
            }
        }
    }
}
