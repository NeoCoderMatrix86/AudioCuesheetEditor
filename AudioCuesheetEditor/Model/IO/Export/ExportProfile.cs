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
    public class ExportProfile
    {
        public static readonly String DefaultFileName = "Export.txt";

        public ExportProfile()
        {
            SchemeHead = new ExportScheme
            {
                SchemeType = SchemeType.Header
            };
            SchemeTracks = new ExportScheme
            {
                SchemeType = SchemeType.Body
            };
            SchemeFooter = new ExportScheme
            {
                SchemeType = SchemeType.Footer
            };
            FileName = DefaultFileName;
            var random = new Random();
            Name = String.Format("{0}_{1}", nameof(ExportProfile), random.Next(1, 100));
        }
        public String Name { get; set; }
        public ExportScheme SchemeHead { get; set; }
        public ExportScheme SchemeTracks { get; set; }
        public ExportScheme SchemeFooter { get; set; }
        public String FileName { get; set; }
        [JsonIgnore]
        public Boolean IsExportable
        {
            get
            {
                if ((SchemeHead != null) && (SchemeHead.IsValid == false))
                {
                    return SchemeHead.IsValid;
                }
                if ((SchemeTracks != null) && (SchemeTracks.IsValid == false))
                {
                    return SchemeTracks.IsValid;
                }
                if ((SchemeFooter != null) && (SchemeFooter.IsValid == false))
                {
                    return SchemeFooter.IsValid;
                }
                return true;
            }
        }
        public byte[] GenerateExport(Cuesheet cuesheet)
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
