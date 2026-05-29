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
using System.Text.Json.Serialization;

namespace AudioCuesheetEditor.Model.IO.Audio
{
    public class Audiofile()
    {
        public static readonly AudioCodec AudioCodecWEBM = new("audio/webm", ".webm", "AudioCodec WEBM");

        public static readonly List<AudioCodec> AudioCodecs =
        [
            AudioCodecWEBM,
            new AudioCodec("audio/mpeg", ".mp3", "AudioCodec MP3"),
            new AudioCodec("audio/ogg", ".oga", "AudioCodec OGA"),
            new AudioCodec("audio/ogg", ".ogg", "AudioCodec OGG"),
            new AudioCodec("audio/ogg; codecs=opus", ".opus", "AudioCodec OPUS"),
            new AudioCodec("audio/wav", ".wav", "AudioCodec WAV"),
            new AudioCodec("audio/wav", ".wave", "AudioCodec WAVE"),
            new AudioCodec("audio/flac", ".flac", "AudioCodec FLAC")
        ];

        private AudioCodec? _audioCodec;
        private String? _name;
        
        [JsonConstructor]
        public Audiofile(String? name) : this()
        {
            _name = name;
        }
        //TODO: Remove constructors?!
        public Audiofile(String name, String objectURL, AudioCodec? audioCodec, TimeSpan? duration = null) : this(name)
        {
            if (String.IsNullOrEmpty(objectURL))
            {
                throw new ArgumentNullException(nameof(objectURL));
            }
            ObjectURL = objectURL;
            AudioCodec = audioCodec;
            Duration = duration;
        }

        public String? Name
        {
            get => _name;
            set
            {
                var extension = Path.GetExtension(value);
                if (extension?.Equals(_audioCodec?.FileExtension, StringComparison.CurrentCultureIgnoreCase) == false)
                {
                    value = $"{value}{_audioCodec?.FileExtension}";
                }
                _name = value;
            }
        }
        [JsonIgnore]
        public String? ObjectURL { get; set; }
        /// <summary>
        /// Duration of the audio file
        /// </summary>
        public TimeSpan? Duration { get; set; }

        public AudioCodec? AudioCodec
        {
            get { return _audioCodec; }
            set
            {
                _audioCodec = value;
                if ((_audioCodec != null) && (Name?.EndsWith(_audioCodec.FileExtension) == false))
                {
                    //Replace file ending
                    Name = String.Format("{0}{1}", Path.GetFileNameWithoutExtension(Name), _audioCodec.FileExtension);
                }
            }
        }
    }
}
