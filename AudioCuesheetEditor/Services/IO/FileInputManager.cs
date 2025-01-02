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
using AudioCuesheetEditor.Model.IO.Audio;
using Microsoft.AspNetCore.Components.Forms;
using Microsoft.JSInterop;

namespace AudioCuesheetEditor.Services.IO
{
    public class FileInputManager(IJSRuntime jsRuntime, HttpClient httpClient)
    {
        private readonly IJSRuntime _jsRuntime = jsRuntime;
        private readonly HttpClient _httpClient = httpClient;

        public static AudioCodec? GetAudioCodec(IBrowserFile browserFile)
        {
            AudioCodec? foundAudioCodec = null;
            var extension = Path.GetExtension(browserFile.Name);
            // First search with mime type and file extension
            var audioCodecsFound = Audiofile.AudioCodecs.Where(x => x.MimeType.Equals(browserFile.ContentType, StringComparison.OrdinalIgnoreCase) && x.FileExtension.Equals(extension, StringComparison.OrdinalIgnoreCase));
            if (audioCodecsFound.Count() <= 1)
            {
                foundAudioCodec = audioCodecsFound.FirstOrDefault();
            }
            else
            {
                // Second search with mime type or file extension
                audioCodecsFound = Audiofile.AudioCodecs.Where(x => x.MimeType.Equals(browserFile.ContentType, StringComparison.OrdinalIgnoreCase) || x.FileExtension.Equals(extension, StringComparison.OrdinalIgnoreCase));
                foundAudioCodec = audioCodecsFound.FirstOrDefault();
            }
            return foundAudioCodec;
        }
        public async Task<Audiofile> CreateAudiofileAsync(String fileInputId, IBrowserFile browserFile)
        {
            //TODO: Check file mime type
            //TODO: RevokeObjectURL of previous audiofile
            var audioFileObjectURL = await _jsRuntime.InvokeAsync<String>("getObjectURLFromMudFileUpload", fileInputId);
            var codec = GetAudioCodec(browserFile);
            var audiofile = new Audiofile(browserFile.Name, audioFileObjectURL, codec);
            if (String.IsNullOrEmpty(audioFileObjectURL) == false)
            {
                _ = _httpClient.GetStreamAsync(audioFileObjectURL).ContinueWith(x => audiofile.ContentStream = x.Result);
            }
            return audiofile;
        }

        public CDTextfile CreateCDTextfile(IBrowserFile browserFile)
        {
            //TODO: Check file mime type
            return new CDTextfile(browserFile.Name);
        }
    }
}
