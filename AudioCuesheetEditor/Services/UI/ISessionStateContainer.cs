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
using AudioCuesheetEditor.Model.IO.Import;

namespace AudioCuesheetEditor.Services.UI
{
    public interface ISessionStateContainer
    {
        public event EventHandler? CuesheetChanged;
        public event EventHandler? ImportCuesheetChanged;
        public Cuesheet Cuesheet { get; set; }
        public Cuesheet? ImportCuesheet { get; set; }
        public Audiofile? ImportAudiofile { get; set; }
        public IImportfile? Importfile { get; set; }
        public void ResetImport();
        public int ImportTabActiveTab { get; set; }
    }
}
