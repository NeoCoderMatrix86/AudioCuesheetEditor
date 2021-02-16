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
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace AudioCuesheetEditor.Model.IO.Audio
{
    public class AudioFile
    {
        public static readonly String RecordingFileName = "Recording";

        public static readonly List<AudioCodec> AudioCodecs = new List<AudioCodec>()
        {
            new AudioCodec("audio/mpeg", ".mp3", "AudioCodec MP3"),
            new AudioCodec("audio/ogg", ".oga", "AudioCodec OGA"),
            new AudioCodec("audio/ogg", ".ogg", "AudioCodec OGG"),
            new AudioCodec("audio/ogg; codecs=opus", ".opus", "AudioCodec OPUS"),
            new AudioCodec("audio/wav", ".wav", "AudioCodec WAV"),
            new AudioCodec("audio/wav", ".wave", "AudioCodec WAVE"),
            new AudioCodec("audio/flac", ".flac", "AudioCodec FLAC")
        };
        
        public AudioFile(String fileName, Boolean isRecorded = false)
        {
            if (String.IsNullOrEmpty(fileName))
            {
                throw new ArgumentNullException(nameof(fileName));
            }
            FileName = fileName;
            IsRecorded = isRecorded;
        }

        public AudioFile(String fileName, String objectURL, String contentType, Boolean isRecorded = false) : this(fileName, isRecorded)
        {
            if (String.IsNullOrEmpty(objectURL))
            {
                throw new ArgumentNullException(nameof(objectURL));
            }
            if (String.IsNullOrEmpty(contentType))
            {
                throw new ArgumentNullException(nameof(contentType));
            }
            ObjectURL = objectURL;
            ContentType = contentType;
        }

        public String FileName { get; private set; }
        public String ObjectURL { get; private set; }
        public String ContentType { get; private set; }
        public String AudioFileType
        {
            get { return Path.GetExtension(FileName).Replace(".", "").ToUpper(); }
        }
        public Boolean PlaybackPossible
        {
            get
            {
                Boolean playbackPossible = false;
                if ((String.IsNullOrEmpty(FileName) == false) && (String.IsNullOrEmpty(ObjectURL) == false) && (String.IsNullOrEmpty(AudioFileType) == false) && ((String.IsNullOrEmpty(ContentType) == true) || (ContentType.StartsWith("audio/"))))
                {
                    playbackPossible = true;
                }
                return playbackPossible;
            }
        }
        public Boolean IsRecorded { get; private set; }
    }
}
