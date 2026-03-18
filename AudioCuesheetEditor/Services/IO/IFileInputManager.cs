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

namespace AudioCuesheetEditor.Services.IO
{
    public interface IFileInputManager
    {
        bool IsValidAudiofile(string? fileContentType, string fileName);
        AudioCodec? GetAudioCodec(string? fileContentType, string fileName);
        /// <summary>
        /// Checks if a file content type and name matches given parameters
        /// </summary>
        /// <param name="fileContentType"></param>
        /// <param name="fileName"></param>
        /// <param name="mimeType"></param>
        /// <param name="fileExtensions"></param>
        /// <returns></returns>
        bool CheckFileMimeType(string? fileContentType, string fileName, string mimeType, IEnumerable<string> fileExtensions);
        Task<Audiofile?> CreateAudiofileAsync(FileUpload fileUpload);
        CDTextfile? CreateCDTextfile(string? fileContentType, string fileName);
        /// <summary>
        /// Checks if the file can be used for the import view
        /// </summary>
        /// <param name="browserFile"></param>
        /// <returns></returns>
        bool IsValidForImportView(string? fileContentType, string fileName);
        /// <summary>
        /// Reads the browser file and gets the file content as string
        /// </summary>
        /// <param name="browserFile"></param>
        /// <returns></returns>
        Task<string> ReadFileContentAsync(IBrowserFile browserFile);
        /// <summary>
        /// Generates file upload references for files
        /// </summary>
        /// <param name="browserFiles"></param>
        /// <param name="fileInputId"></param>
        /// <returns></returns>
        Task<IEnumerable<FileUpload>> CreateFileUploadsAsync(IReadOnlyList<IBrowserFile> browserFiles, string? fileInputId = null);
    }
}