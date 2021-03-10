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
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace AudioCuesheetEditor.Model.IO.Audio
{
    public class AudioFile : IDisposable
    {
        public static readonly String RecordingFileName = "Recording.webm";
        public static readonly AudioCodec AudioCodecWEBM = new AudioCodec("audio/webm", ".webm", "AudioCodec WEBM");

        public static readonly List<AudioCodec> AudioCodecs = new List<AudioCodec>()
        {
            AudioCodecWEBM,
            new AudioCodec("audio/mpeg", ".mp3", "AudioCodec MP3"),
            new AudioCodec("audio/ogg", ".oga", "AudioCodec OGA"),
            new AudioCodec("audio/ogg", ".ogg", "AudioCodec OGG"),
            new AudioCodec("audio/ogg; codecs=opus", ".opus", "AudioCodec OPUS"),
            new AudioCodec("audio/wav", ".wav", "AudioCodec WAV"),
            new AudioCodec("audio/wav", ".wave", "AudioCodec WAVE"),
            new AudioCodec("audio/flac", ".flac", "AudioCodec FLAC")
        };

        private AudioCodec audioCodec;
        private bool disposedValue;

        public event EventHandler ContentStreamLoaded;
        
        [JsonConstructor]
        public AudioFile(String fileName, Boolean isRecorded = false)
        {
            if (String.IsNullOrEmpty(fileName))
            {
                throw new ArgumentNullException(nameof(fileName));
            }
            FileName = fileName;
            IsRecorded = isRecorded;
        }

        public AudioFile(String fileName, String objectURL, AudioCodec audioCodec, System.Net.Http.HttpClient httpClient, Boolean isRecorded = false) : this(fileName, isRecorded)
        {
            if (String.IsNullOrEmpty(objectURL))
            {
                throw new ArgumentNullException(nameof(objectURL));
            }
            if (audioCodec == null)
            {
                throw new ArgumentNullException(nameof(audioCodec));
            }
            if (httpClient == null)
            {
                throw new ArgumentNullException(nameof(httpClient));
            }
            ObjectURL = objectURL;
            AudioCodec = audioCodec;
            //Read stream asynchronously in order to be prepared (using large files)
            _ = LoadContentStream(httpClient);
        }

        public String FileName { get; private set; }
        //TODO: Get rid of objecturl and save data inside the contentstream (Memorystream)
        [JsonIgnore]
        public String ObjectURL { get; private set; }
        /// <summary>
        /// Boolean indicating if the stream has fully been loaded
        /// </summary>
        [JsonIgnore]
        public Boolean IsContentStreamLoaded 
        {
            get { return ContentStream != null; }
        }
        /// <summary>
        /// File content stream. Be carefull, this stream is loaded asynchronously. Connect to the StreamLoaded for checking if loading has already been done!
        /// </summary>
        [JsonIgnore]
        public Stream ContentStream { get; private set; }
        [JsonIgnore]
        public Boolean IsRecorded { get; private set; }
        /// <summary>
        /// Duration of the audio file
        /// </summary>
        [JsonConverter(typeof(JsonTimeSpanConverter))]
        public TimeSpan? Duration { get; private set; }

        public AudioCodec AudioCodec 
        {
            get { return audioCodec; }
            private set
            {
                audioCodec = value;
                if ((audioCodec != null) && (FileName.EndsWith(audioCodec.FileExtension) == false))
                {
                    //Replace file ending
                    FileName = String.Format("{0}{1}", Path.GetFileNameWithoutExtension(FileName), AudioCodec.FileExtension);
                }
            }
        }

        [JsonIgnore]
        public String AudioFileType
        {
            get 
            {
                String audioFileType = null;
                if (AudioCodec != null)
                {
                    audioFileType = AudioCodec.FileExtension.Replace(".", "").ToUpper();
                }
                if (audioFileType == null)
                {
                    //Try to find by file name
                    audioFileType = Path.GetExtension(FileName).Replace(".", "").ToUpper();
                }
                return audioFileType;
            }
        }

        [JsonIgnore]
        public Boolean PlaybackPossible
        {
            get
            {
                Boolean playbackPossible = false;
                if ((String.IsNullOrEmpty(FileName) == false) && (String.IsNullOrEmpty(ObjectURL) == false) && (String.IsNullOrEmpty(AudioFileType) == false) && (AudioCodec != null))
                {
                    playbackPossible = true;
                }
                return playbackPossible;
            }
        }

        private async Task LoadContentStream(System.Net.Http.HttpClient httpClient)
        {
            if (String.IsNullOrEmpty(ObjectURL) == false)
            {
                ContentStream = await httpClient.GetStreamAsync(ObjectURL);
                ContentStreamLoaded?.Invoke(this, EventArgs.Empty);
                var track = new ATL.Track(ContentStream, AudioCodec.MimeType);
                Duration = new TimeSpan(0, 0, track.Duration);
            }
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing)
                {
                    ContentStream.Close();
                    ContentStream.Dispose();
                    ContentStream = null;
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
