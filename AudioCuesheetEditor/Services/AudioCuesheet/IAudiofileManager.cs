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
using AudioCuesheetEditor.Model.IO.Audio;
using Microsoft.AspNetCore.Components.Forms;
using System.Linq.Expressions;

namespace AudioCuesheetEditor.Services.AudioCuesheet
{
    public interface IAudiofileManager
    {
        /// <summary>
        /// Set properties from a file upload
        /// </summary>
        /// <param name="audiofile"></param>
        /// <param name="browserFile"></param>
        /// <param name="fileInputId"></param>
        Task SetPropertiesAsync(Audiofile audiofile, IBrowserFile? browserFile, string fileInputId);
        /// <summary>
        /// Set property for an audio file
        /// </summary>
        /// <typeparam name="TProperty"></typeparam>
        /// <param name="audiofile"></param>
        /// <param name="propertyExpression"></param>
        /// <param name="value"></param>
        void SetProperty<TProperty>(Audiofile audiofile, Expression<Func<Audiofile, TProperty>> propertyExpression, TProperty value);
    }
}
