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
using FFmpegBlazor;
using Microsoft.JSInterop;

namespace AudioCuesheetEditor.Model.IO.Audio
{
    public class AudioConverterService : IAudioConverterService, IDisposable
    {
        private readonly IJSRuntime _jSRuntime;
        private FFMPEG? ffMPEGInstance;
        private bool disposedValue;

        public event EventHandler<double>? ProgressChanged;

        public AudioConverterService(IJSRuntime jSRuntime)
        {
            _jSRuntime = jSRuntime;
        }

        public void Dispose()
        {
            // Ändern Sie diesen Code nicht. Fügen Sie Bereinigungscode in der Methode "Dispose(bool disposing)" ein.
            Dispose(disposing: true);
            GC.SuppressFinalize(this);
        }

        public Boolean IsReady => (ffMPEGInstance != null) && ffMPEGInstance.IsLoaded;

        public async Task<byte[]?> SplitAudiofileAsync(Audiofile audiofile, TimeSpan from, TimeSpan to)
        {
            if (IsReady == false)
            {
                await InitAsync();
            }
            if ((ffMPEGInstance != null) && (audiofile.ContentStream != null))
            {
                byte[] buffer = new byte[audiofile.ContentStream.Length];
                await audiofile.ContentStream.ReadAsync(buffer);
                ffMPEGInstance.WriteFile(audiofile.Name, buffer);
                var splitAudiofilename = String.Format("output-{0}{1}", Guid.NewGuid(), audiofile.AudioCodec?.FileExtension);
                await ffMPEGInstance.Run("-ss", from.ToString("hh\\:mm\\:ss\\.fff"), "-i", audiofile.Name, "-t", to.ToString("hh\\:mm\\:ss\\.fff"), "-c", "copy", splitAudiofilename);
                ffMPEGInstance.UnlinkFile(audiofile.Name);
                var res = await ffMPEGInstance.ReadFile(splitAudiofilename);
                return res;
            }
            return null;
        }

        public async Task InitAsync()
        {
            //initialize FFmpegFactory
            await FFmpegFactory.Init(_jSRuntime);
            if (ffMPEGInstance == null)
            {
                ffMPEGInstance = FFmpegFactory.CreateFFmpeg(new FFmpegConfig() { Log = true });
                await ffMPEGInstance.Load(true);

                ffMPEGInstance.Progress += FFMPEGInstance_Progress;
            }
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing)
                {
                    // Verwalteten Zustand (verwaltete Objekte) bereinigen
                    if (ffMPEGInstance != null)
                    {
                        ffMPEGInstance.Progress -= FFMPEGInstance_Progress;
                    }
                    
                }

                // Nicht verwaltete Ressourcen (nicht verwaltete Objekte) freigeben und Finalizer überschreiben
                // Große Felder auf NULL setzen
                disposedValue = true;
            }
        }

        private void FFMPEGInstance_Progress(Progress p)
        {
            ProgressChanged?.Invoke(this, p.Ratio);
        }
    }
}
