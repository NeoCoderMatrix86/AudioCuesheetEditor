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
using AudioCuesheetEditor.Model.UI;

namespace AudioCuesheetEditor.Services.UI
{
    public class SessionStateContainer : ISessionStateContainer, ITraceable
    {
        public event EventHandler? CuesheetChanged;
        public event EventHandler? ImportCuesheetChanged;
        public event EventHandler<TraceablePropertiesChangedEventArgs>? TraceablePropertyChanged;

        private readonly ITraceChangeManager _traceChangeManager;
        private Cuesheet cuesheet;
        private Cuesheet? importCuesheet;
        private Audiofile? importAudiofile;

        public SessionStateContainer(ITraceChangeManager traceChangeManager)
        {
            _traceChangeManager = traceChangeManager;
            cuesheet = new Cuesheet();
            SetCuesheetReference(cuesheet);
            _traceChangeManager.TraceChanges(this);
        }
        public Cuesheet Cuesheet 
        {
            get { return cuesheet; }
            set { SetCuesheetReference(value); }
        }
        public Cuesheet? ImportCuesheet
        {
            get { return importCuesheet; }
            set
            {
                var previousValue = importCuesheet;
                importCuesheet = value;
                //When there is an audiofile from import, we use this file because it has an object url and gets duration, etc.
                if (importCuesheet != null && ImportAudiofile != null)
                {
                    importCuesheet.Audiofile = ImportAudiofile;
                }
                if (Equals(previousValue, importCuesheet) == false)
                {
                    ImportCuesheetChanged?.Invoke(this, EventArgs.Empty);
                }
            }
        }

        public Audiofile? ImportAudiofile
        {
            get => importAudiofile;
            set
            {
                importAudiofile = value;
                if (ImportCuesheet != null && ImportAudiofile != null)
                {
                    ImportCuesheet.Audiofile = ImportAudiofile;
                    ImportCuesheetChanged?.Invoke(this, EventArgs.Empty);
                }
            }
        }

        public IImportfile? Importfile{ get; set; }
        public int ImportTabActiveTab { get; set; } = 0;

        public void ResetImport()
        {
            Importfile = null;
            ImportAudiofile = null;
            ImportCuesheet = null;
        }

        private void SetCuesheetReference(Cuesheet value)
        {
            var previousValue = Cuesheet;
            cuesheet = value;
            _traceChangeManager.TraceChanges(Cuesheet);
            TraceablePropertyChanged?.Invoke(this, new TraceablePropertiesChangedEventArgs(new TraceableChange(previousValue, nameof(Cuesheet))));
            CuesheetChanged?.Invoke(this, EventArgs.Empty);
        }
    }
}
