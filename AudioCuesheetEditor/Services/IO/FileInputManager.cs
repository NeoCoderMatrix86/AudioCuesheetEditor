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
using AudioCuesheetEditor.Model.IO;
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

        public static Boolean CheckFileMimeType(IBrowserFile file, String mimeType, String fileExtension)
        {
            Boolean fileMimeTypeMatches = false;
            if ((file != null) && (String.IsNullOrEmpty(mimeType) == false) && (String.IsNullOrEmpty(fileExtension) == false))
            {
                if (String.IsNullOrEmpty(file.ContentType) == false)
                {
                    fileMimeTypeMatches = file.ContentType.Equals(mimeType, StringComparison.CurrentCultureIgnoreCase);
                }
                else
                {
                    //Try to find by file extension
                    var extension = Path.GetExtension(file.Name);
                    fileMimeTypeMatches = extension.Equals(fileExtension, StringComparison.CurrentCultureIgnoreCase);
                }
            }
            return fileMimeTypeMatches;
        }

        public async Task<Audiofile?> CreateAudiofileAsync(String? fileInputId, IBrowserFile? browserFile)
        {
            Audiofile? audiofile = null;
            if ((String.IsNullOrEmpty(fileInputId) == false) && (browserFile != null))
            {
                // Check file mime type
                var codec = GetAudioCodec(browserFile);
                if (codec != null)
                {
                    var audioFileObjectURL = await _jsRuntime.InvokeAsync<String>("getObjectURLFromMudFileUpload", fileInputId);
                    audiofile = new Audiofile(browserFile.Name, audioFileObjectURL, codec);
                    if (String.IsNullOrEmpty(audioFileObjectURL) == false)
                    {
                        _ = _httpClient.GetStreamAsync(audioFileObjectURL).ContinueWith(x => audiofile.ContentStream = x.Result);
                    }
                }
                else
                {
                    throw new ArgumentException("The audiofile provided is not of a valid type.");
                }
            }
            return audiofile;
        }

        public static CDTextfile? CreateCDTextfile(IBrowserFile? browserFile)
        {
            CDTextfile? cdTextfile = null;
            if (browserFile != null)
            {
                if (CheckFileMimeType(browserFile, FileMimeTypes.CDTextfile, FileExtensions.CDTextfile))
                {
                    cdTextfile = new CDTextfile(browserFile.Name);
                }
                else
                {
                    throw new ArgumentException("The cdtextfile provided is not of a valid type.");
                }
            }
            return cdTextfile;
        }
    }
}
