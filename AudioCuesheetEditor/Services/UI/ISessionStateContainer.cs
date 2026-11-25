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

namespace AudioCuesheetEditor.Services.UI;

public interface ISessionStateContainer
{
    event EventHandler? CuesheetChanged;
    event EventHandler? ActiveCuesheetChanged;
    Cuesheet Cuesheet { get; set; }
    Cuesheet? ImportCuesheet { get; set; }
    IList<Audiofile> ImportAudiofiles { get; set; }
    Dictionary<Audiofile, Audiofile?> ImportAudiofileMapping { get; set; }
    IImportfile? Importfile { get; set; }
    bool ImportIsAnalyzed { get; set; }
    Task ResetImportAsync(bool revokeObjectURLs);
    Task ResetAudioImportAsync(bool revokeObjectURLs);
    /// <summary>
    /// Initialize this class
    /// </summary>
    /// <returns></returns>
    Task InitializeAsync();
    /// <summary>
    /// Get current cuesheet based on current view
    /// </summary>
    Cuesheet? ActiveCuesheet { get; }
}
