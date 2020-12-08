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
using System.Threading.Tasks;

namespace AudioCuesheetEditor.Model.IO
{
    public class CuesheetFile
    {
        public static readonly String CuesheetArtist = "PERFORMER";
        public static readonly String CuesheetTitle = "TITLE";
        public static readonly String CuesheetFileName = "FILE";
        public static readonly String CuesheetTrack = "TRACK";
        public static readonly String CuesheetTrackAudio = "AUDIO";
        public static readonly String TrackArtist = "PERFORMER";
        public static readonly String TrackTitle = "TITLE";
        public static readonly String TrackIndex01 = "INDEX 01";

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
            //TODO: Generation
            byte[] cuesheetFile = null;
            

            return cuesheetFile;
        }
    }
}
