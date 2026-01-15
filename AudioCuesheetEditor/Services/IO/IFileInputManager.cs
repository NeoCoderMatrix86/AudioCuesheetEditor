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

namespace AudioCuesheetEditor.Services.IO
{
    public interface IFileInputManager
    {
        bool IsValidAudiofile(IBrowserFile browserFile);
        AudioCodec? GetAudioCodec(IBrowserFile browserFile);
        bool CheckFileMimeType(IBrowserFile file, string mimeType, IEnumerable<string> fileExtensions);
        Task<Audiofile?> CreateAudiofileAsync(string? fileInputId, IBrowserFile? browserFile, Action<Task<Stream>>? afterContentStreamLoaded = null);
        CDTextfile? CreateCDTextfile(IBrowserFile? browserFile);
        /// <summary>
        /// Checks if the file can be used for the import view
        /// </summary>
        /// <param name="browserFile"></param>
        /// <returns></returns>
        bool IsValidForImportView(IBrowserFile browserFile);
        /// <summary>
        /// Reads the browser file and gets the file content as string
        /// </summary>
        /// <param name="browserFile"></param>
        /// <returns></returns>
        Task<string> ReadFileContentAsync(IBrowserFile browserFile);
    }
}