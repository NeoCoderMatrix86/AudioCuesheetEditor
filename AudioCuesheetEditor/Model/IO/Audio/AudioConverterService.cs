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
using System.Runtime.CompilerServices;

namespace AudioCuesheetEditor.Model.IO.Audio
{
    public class AudioConverterService
    {
        private readonly IJSRuntime _jSRuntime;
        private FFMPEG? ffMPEGInstance;

        public AudioConverterService(IJSRuntime jSRuntime)
        {
            _jSRuntime = jSRuntime;
        }

        public Boolean IsReady => (ffMPEGInstance != null) && ffMPEGInstance.IsLoaded;

        public async Task<byte[]?> SplitAudiofileAsync(Audiofile audiofile, TimeSpan from, TimeSpan to)
        {
            //TODO: absichern!
            if (IsReady == false)
            {
                await InitAsync();
            }
            if ((ffMPEGInstance != null) && (audiofile.ContentStream != null))
            {
                byte[] buffer = new byte[audiofile.ContentStream.Length];
                await audiofile.ContentStream.ReadAsync(buffer);
                ffMPEGInstance.WriteFile(audiofile.Name, buffer);
                //var newFileName = String.Format("{0}-part1.{1}", Path.GetFileNameWithoutExtension(audiofile.FileName), audiofile.AudioFileType);
                //ffMPEG.WriteFile($"{newFileName}", buffer);
                //await ffMPEGInstance.Run("-i", audiofile.Name, "-f", "segment", "-segment_time", "3", "-c", "copy out%03d.mp3");
                var splitAudiofilename = String.Format("output-{0}{1}", Guid.NewGuid(), audiofile.AudioCodec?.FileExtension);
                await ffMPEGInstance.Run("-ss", from.ToString("hh\\:mm\\:ss\\.fff"), "-i", audiofile.Name, "-t", to.ToString("hh\\:mm\\:ss\\.fff"), "-c", "copy", splitAudiofilename);
                ffMPEGInstance.UnlinkFile(audiofile.Name);
                var res = await ffMPEGInstance.ReadFile(splitAudiofilename);
                return res;
            }
            return null;
        }

        private async Task InitAsync()
        {
            //initialize FFmpegFactory
            await FFmpegFactory.Init(_jSRuntime);

            ffMPEGInstance = FFmpegFactory.CreateFFmpeg(new FFmpegConfig() { Log = true });
            await ffMPEGInstance.Load();
        }
    }
}
