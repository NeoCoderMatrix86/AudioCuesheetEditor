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
using Microsoft.AspNetCore.Components.WebAssembly.Http;
using Microsoft.JSInterop;

namespace AudioCuesheetEditor.Services.IO
{
    public class FileInputManager(IJSRuntime jsRuntime, HttpClient httpClient, ILogger<FileInputManager> logger) : IFileInputManager
    {
        private readonly IJSRuntime _jsRuntime = jsRuntime;
        private readonly HttpClient _httpClient = httpClient;
        private readonly ILogger<FileInputManager> _logger = logger;

        public AudioCodec? GetAudioCodec(string? fileContentType, string fileName)
        {
            AudioCodec? foundAudioCodec = null;
            var extension = Path.GetExtension(fileName);
            // First search with mime type and file extension
            var audioCodecsFound = Audiofile.AudioCodecs.Where(x => x.MimeType.Equals(fileContentType, StringComparison.OrdinalIgnoreCase) && x.FileExtension.Equals(extension, StringComparison.OrdinalIgnoreCase));
            if (audioCodecsFound.Count() <= 1)
            {
                foundAudioCodec = audioCodecsFound.FirstOrDefault();
            }
            if (foundAudioCodec == null)
            {
                // Second search with mime type or file extension
                audioCodecsFound = Audiofile.AudioCodecs.Where(x => x.MimeType.Equals(fileContentType, StringComparison.OrdinalIgnoreCase) || x.FileExtension.Equals(extension, StringComparison.OrdinalIgnoreCase));
                foundAudioCodec = audioCodecsFound.FirstOrDefault();
            }
            return foundAudioCodec;
        }

        public bool IsValidAudiofile(string? fileContentType, string fileName)
        {
            return GetAudioCodec(fileContentType, fileName) != null;
        }

        public bool CheckFileMimeType(string? fileContentType, string fileName, string mimeType, IEnumerable<string> fileExtensions)
        {
            if (_logger.IsEnabled(LogLevel.Debug))
            {
                _logger.LogDebug("CheckFileMimeType called with file: file.Name: '{FileName}', file.ContentType: '{ContentType}', mimeType: '{MimeType}', fileExtensions: '{fileExtensions}'", fileName, fileContentType, mimeType, fileExtensions);
            }
            Boolean fileMimeTypeMatches = false;
            if (String.IsNullOrEmpty(mimeType) == false)
            {
                if (String.IsNullOrEmpty(fileContentType) == false)
                {
                    if (mimeType.EndsWith("/*"))
                    {
                        var mainType = mimeType[..^1];
                        fileMimeTypeMatches = fileContentType.StartsWith(mainType, StringComparison.CurrentCultureIgnoreCase);
                    }
                    else
                    {
                        fileMimeTypeMatches = fileContentType.Equals(mimeType, StringComparison.CurrentCultureIgnoreCase);
                    }
                }
                if ((fileMimeTypeMatches == false) && fileExtensions.Any())
                {
                    //Try to find by file extension
                    var extension = Path.GetExtension(fileName);
                    fileMimeTypeMatches = fileExtensions.Any(x => x.Equals(extension, StringComparison.CurrentCultureIgnoreCase));
                }
            }
            return fileMimeTypeMatches;
        }

        public async Task<Audiofile?> CreateAudiofileAsync(FileUpload fileUpload)
        {
            Audiofile? audiofile = null;
            if (fileUpload.ObjectUrl != null)
            {
                // Check file mime type
                var codec = GetAudioCodec(fileUpload.ContentType, fileUpload.Name);
                if (codec != null)
                {
                    TimeSpan? duration = null;
                    if (String.IsNullOrEmpty(fileUpload.ObjectUrl) == false)
                    {
                        var durationSeconds = await _jsRuntime.InvokeAsync<double>("getAudioDurationFromFile", fileUpload.ObjectUrl);
                        duration = TimeSpan.FromSeconds(durationSeconds);
                    }
                    audiofile = new Audiofile(fileUpload.Name, fileUpload.ObjectUrl, codec, duration);
                }
                else
                {
                    throw new ArgumentException("The audiofile provided is not of a valid type.");
                }
            }
            return audiofile;
        }

        public CDTextfile? CreateCDTextfile(string? fileContentType, string fileName)
        {
            CDTextfile? cdTextfile;
            if (CheckFileMimeType(fileContentType, fileName, FileMimeTypes.Text, [FileExtensions.CDTextfile]))
            {
                cdTextfile = new CDTextfile(fileName);
            }
            else
            {
                throw new ArgumentException("The cdtextfile provided is not of a valid type.");
            }
            return cdTextfile;
        }

        /// <inheritdoc/>
        public bool IsValidForImportView(string? fileContentType, string fileName)
        {
            return CheckFileMimeType(fileContentType, fileName, FileMimeTypes.Text, [FileExtensions.Text, FileExtensions.HTML]);
        }

        /// <inheritdoc/>
        public async Task<string> ReadFileContentAsync(IBrowserFile browserFile)
        {
            var fileContent = new StreamContent(browserFile.OpenReadStream());
            return await fileContent.ReadAsStringAsync();
        }

        /// <inheritdoc/>
        public async Task<IEnumerable<FileUpload>> CreateFileUploadsAsync(IReadOnlyList<IBrowserFile> browserFiles, string? fileInputId = null)
        {
            List<FileUpload> fileUploads = [];
            foreach (var file in browserFiles)
            {
                if (CheckFileMimeType(file.ContentType, file.Name, FileMimeTypes.Projectfile, [FileExtensions.Projectfile])
                    || CheckFileMimeType(file.ContentType, file.Name, FileMimeTypes.Cuesheet, [FileExtensions.Cuesheet])
                    || IsValidForImportView(file.ContentType, file.Name)
                    || IsValidAudiofile(file.ContentType, file.Name))
                {
                    string? content = null;
                    string? objectUrl = null;
                    if (IsValidAudiofile(file.ContentType, file.Name))
                    {
                        objectUrl = await _jsRuntime.InvokeAsync<String>("getObjectURLFromMudFileUpload", fileInputId);
                    }
                    else
                    {
                        content = await ReadFileContentAsync(file);
                    }
                    fileUploads.Add(new(file.Name, file.ContentType, content, objectUrl));
                }
            }
            return fileUploads;
        }
    }
}
