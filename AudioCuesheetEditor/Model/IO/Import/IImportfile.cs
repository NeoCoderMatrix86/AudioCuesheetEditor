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
using AudioCuesheetEditor.Model.AudioCuesheet.Import;
using AudioCuesheetEditor.Services.IO;

namespace AudioCuesheetEditor.Model.IO.Import
{
    public interface IImportfile
    {
        /// <summary>
        /// File content
        /// </summary>
        String? FileContent { get; set; }
        /// <summary>
        /// File content with marking which passages has been recognized by scheme
        /// </summary>
        String? FileContentRecognized { get; set; }
        /// <summary>
        /// Exception that has been thrown while reading out the file
        /// </summary>
        Exception? AnalyseException { get; set; }
        /// <summary>
        /// The cue sheet which was created during analyzing the <see cref="FileContent"/>
        /// </summary>
        ImportCuesheet? AnalyzedCuesheet { get; set; }
        ImportFileType FileType { get; set; }
    }
}
