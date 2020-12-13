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
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AudioCuesheetEditor.Model.IO
{
    public class CuesheetFile
    {
        public static readonly String DefaultFileName = "Cuesheet.cue";

        public static readonly String CuesheetArtist = "PERFORMER";
        public static readonly String CuesheetTitle = "TITLE";
        public static readonly String CuesheetFileName = "FILE";
        public static readonly String CuesheetTrack = "TRACK";
        public static readonly String CuesheetTrackAudio = "AUDIO";
        public static readonly String TrackArtist = "PERFORMER";
        public static readonly String TrackTitle = "TITLE";
        public static readonly String TrackIndex01 = "INDEX 01";
        public static readonly String Tab = "\t";

        public CuesheetFile(Cuesheet cuesheet)
        {
            if (cuesheet == null)
            {
                throw new ArgumentNullException(nameof(cuesheet));
            }
            Cuesheet = cuesheet;
        }
        public Cuesheet Cuesheet { get; private set; }

        public byte[] GenerateCuesheetFile()
        {
            if (IsExportable == true)
            {
                var builder = new StringBuilder();
                builder.AppendLine(String.Format("{0} \"{1}\"", CuesheetTitle, Cuesheet.Title));
                builder.AppendLine(String.Format("{0} \"{1}\"", CuesheetArtist, Cuesheet.Artist));
                builder.AppendLine(String.Format("{0} \"{1}\" {2}", CuesheetFileName, Cuesheet.AudioFile.FileName, Cuesheet.AudioFile.AudioFileType));
                foreach (var track in Cuesheet.Tracks)
                {
                    builder.AppendLine(String.Format("{0}{1} {2:00} {3}", Tab, CuesheetTrack, track.Position, CuesheetTrackAudio));
                    builder.AppendLine(String.Format("{0}{1}{2} \"{3}\"", Tab, Tab, TrackTitle, track.Title));
                    builder.AppendLine(String.Format("{0}{1}{2} \"{3}\"", Tab, Tab, TrackArtist, track.Artist));
                    builder.AppendLine(String.Format("{0}{1}{2} {3:00}:{4:00}:{5:00}", Tab, Tab, TrackIndex01, Math.Floor(track.Begin.Value.TotalMinutes), track.Begin.Value.Seconds, track.Begin.Value.Milliseconds / 75));
                }
                return Encoding.UTF8.GetBytes(builder.ToString());
            }
            else
            {
                return null;
            }
        }

        public Boolean IsExportable
        {
            get
            {
                if (Cuesheet.GetValidationErrorsFiltered(validationErrorFilterType: Entity.ValidationErrorFilterType.ErrorOnly).Count() > 0)
                {
                    return false;
                }
                foreach(var track in Cuesheet.Tracks)
                {
                    if (track.GetValidationErrorsFiltered(validationErrorFilterType: Entity.ValidationErrorFilterType.ErrorOnly).Count() > 0)
                    {
                        return false;
                    }
                }
                return true;
            }
        }
    }
}
