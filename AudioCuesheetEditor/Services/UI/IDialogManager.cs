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

namespace AudioCuesheetEditor.Services.UI
{
    public interface IDialogManager
    {
        void HideLoadingDialog();
        Task ShowAndHandleModalEditDialogAsync(IEnumerable<Track> tracks);
        Task<bool> ShowImportAudiofilesDialogAsync();
        Task ShowLoadingDialogAsync();
        /// <summary>
        /// Show input dialog and wait for user input.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="title"></param>
        /// <param name="placeholder"></param>
        /// <param name="label"></param>
        /// <param name="initialValue"></param>
        /// <param name="OkText"></param>
        /// <param name="OkButtonStartIcon"></param>
        /// <returns></returns>
        Task<Result<T>> ShowInputDialogAsync<T>(string? title, string? placeholder, string? label, T initialValue, string? OkText = "Ok", string? OkButtonStartIcon = null);
    }
}