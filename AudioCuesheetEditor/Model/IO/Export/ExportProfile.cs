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
using AudioCuesheetEditor.Shared.ResourceFiles;
using Microsoft.Extensions.Localization;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AudioCuesheetEditor.Model.IO.Export
{
    public class ExportProfile
    {
        public static readonly String DefaultFileName = "Export.txt";

        private readonly IStringLocalizer<Localization> localizer;

        public ExportProfile(IStringLocalizer<Localization> localizer, Cuesheet cuesheet)
        {
            if (cuesheet == null)
            {
                throw new ArgumentNullException(nameof(cuesheet));
            }
            if (localizer == null)
            {
                throw new ArgumentNullException(nameof(localizer));
            }
            this.localizer = localizer;
            Cuesheet = cuesheet;
            SchemeHead = new ExportScheme(this.localizer, SchemeType.Header);
            SchemeTracks = new ExportScheme(this.localizer, SchemeType.Body);
            SchemeFooter = new ExportScheme(this.localizer, SchemeType.Footer);
            FileName = DefaultFileName;
            Name = nameof(ExportProfile);
        }
        public Cuesheet Cuesheet { get; private set; }
        public String Name { get; set; }
        public ExportScheme SchemeHead { get; private set; }
        public ExportScheme SchemeTracks { get; private set; }
        public ExportScheme SchemeFooter { get; private set; }
        public String FileName { get; set; }
        public Boolean IsExportable
        {
            get
            {
                if (SchemeHead.IsValid == false)
                {
                    return SchemeHead.IsValid;
                }
                if (SchemeTracks.IsValid == false)
                {
                    return SchemeTracks.IsValid;
                }
                if (SchemeFooter.IsValid == false)
                {
                    return SchemeFooter.IsValid;
                }
                return true;
            }
        }
        public byte[] GenerateExport()
        {
            if (IsExportable == true)
            {
                var builder = new StringBuilder();
                builder.AppendLine(SchemeHead.GetExportResult(Cuesheet));
                foreach(var track in Cuesheet.Tracks)
                {
                    builder.AppendLine(SchemeTracks.GetExportResult(track));
                }
                builder.AppendLine(SchemeFooter.GetExportResult(Cuesheet));
                return Encoding.UTF8.GetBytes(builder.ToString());
            }
            else
            {
                return null;
            }
        }
    }
}
