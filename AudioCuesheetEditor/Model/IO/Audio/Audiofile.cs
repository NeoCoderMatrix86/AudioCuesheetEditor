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
    [method: JsonConstructor]
    public class Audiofile(String name) : IDisposable, IAudiofile
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

        private AudioCodec? audioCodec;
        private Stream? contentStream;
        private String name = name;
        private bool disposedValue;

        public event EventHandler? ContentStreamLoaded;

        public Audiofile(String name, String objectURL, AudioCodec? audioCodec) : this(name)
        {
            if (String.IsNullOrEmpty(objectURL))
            {
                throw new ArgumentNullException(nameof(objectURL));
            }
            ObjectURL = objectURL;
            AudioCodec = audioCodec;
        }

        public String Name
        {
            get => name;
            set
            {
                if (String.IsNullOrEmpty(value))
                {
                    throw new ArgumentNullException(nameof(value));
                }
                var extension = Path.GetExtension(value);
                if (extension.Equals(audioCodec?.FileExtension, StringComparison.CurrentCultureIgnoreCase) == false)
                {
                    value = $"{value}{audioCodec?.FileExtension}";
                }
                name = value;
            }
        }
        [JsonIgnore]
        public String? ObjectURL { get; private set; }
        /// <summary>
        /// Boolean indicating if the stream has fully been loaded
        /// </summary>
        [JsonIgnore]
        public Boolean IsContentStreamLoaded
        {
            get { return ContentStream != null; }
        }
        /// <summary>
        /// File content stream. Be carefully, this stream is loaded asynchronously. Connect to the StreamLoaded for checking if loading has already been done!
        /// </summary>
        [JsonIgnore]
        public Stream? ContentStream
        {
            get => contentStream;
            set
            {
                contentStream = value;
                if ((ContentStream != null) && (AudioCodec != null))
                {
                    var track = new ATL.Track(ContentStream, AudioCodec.MimeType);
                    Duration = new TimeSpan(0, 0, track.Duration);
                    ContentStreamLoaded?.Invoke(this, EventArgs.Empty);
                }
            }
        }
        /// <summary>
        /// Duration of the audio file
        /// </summary>
        public TimeSpan? Duration { get; private set; }

        public AudioCodec? AudioCodec
        {
            get { return audioCodec; }
            private set
            {
                audioCodec = value;
                if ((audioCodec != null) && (Name?.EndsWith(audioCodec.FileExtension) == false))
                {
                    //Replace file ending
                    Name = String.Format("{0}{1}", Path.GetFileNameWithoutExtension(Name), audioCodec.FileExtension);
                }
            }
        }

        [JsonIgnore]
        public String? AudioFileType
        {
            get
            {
                String? audioFileType = null;
                if (AudioCodec != null)
                {
                    audioFileType = AudioCodec.FileExtension.Replace(".", "").ToUpper();
                }
                //Try to find by file name
                audioFileType ??= Path.GetExtension(Name)?.Replace(".", "").ToUpper();
                return audioFileType;
            }
        }

        [JsonIgnore]
        public Boolean PlaybackPossible
        {
            get
            {
                Boolean playbackPossible = false;
                if ((String.IsNullOrEmpty(Name) == false) && (String.IsNullOrEmpty(ObjectURL) == false) && (String.IsNullOrEmpty(AudioFileType) == false) && (AudioCodec != null))
                {
                    playbackPossible = true;
                }
                return playbackPossible;
            }
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing)
                {
                    if (ContentStream != null)
                    {
                        ContentStream.Close();
                        ContentStream.Dispose();
                        ContentStream = null;
                    }
                }

                disposedValue = true;
            }
        }

        public void Dispose()
        {
            // Ändern Sie diesen Code nicht. Fügen Sie Bereinigungscode in der Methode "Dispose(bool disposing)" ein.
            Dispose(disposing: true);
            GC.SuppressFinalize(this);
        }
    }
}
