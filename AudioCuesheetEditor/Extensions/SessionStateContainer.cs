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
using AudioCuesheetEditor.Model.AudioCuesheet.Import;
using AudioCuesheetEditor.Model.IO.Audio;
using AudioCuesheetEditor.Model.IO.Import;
using AudioCuesheetEditor.Model.Options;
using AudioCuesheetEditor.Model.UI;

namespace AudioCuesheetEditor.Extensions
{
    public class SessionStateContainer
    {
        public event EventHandler? CurrentViewModeChanged;
        public event EventHandler? CuesheetChanged;
        public event EventHandler? ImportCuesheetChanged;

        private readonly TraceChangeManager _traceChangeManager;
        private ViewMode currentViewMode;
        private Cuesheet cuesheet;
        private Cuesheet? importCuesheet;
        private CuesheetImportfile? cuesheetImportFile;
        private Audiofile? importAudiofile;

        public SessionStateContainer(TraceChangeManager traceChangeManager)
        {
            _traceChangeManager = traceChangeManager;
            cuesheet = new Cuesheet(_traceChangeManager);
            SetCuesheetReference(cuesheet);
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
                if ((importCuesheet != null) && (ImportAudiofile != null))
                {
                    importCuesheet.Audiofile = ImportAudiofile;
                }
                if (Object.Equals(previousValue, importCuesheet) == false)
                {
                    ImportCuesheetChanged?.Invoke(this, EventArgs.Empty);
                }
            }
        }

        public TextImportfile? TextImportFile { get; set; }
        
        public CuesheetImportfile? CuesheetImportFile
        {
            get { return cuesheetImportFile; }
            set
            {
                if (cuesheetImportFile != null)
                {
                    cuesheetImportFile.AnalysisFinished -= CuesheetImportFile_AnalysisFinished;
                }
                cuesheetImportFile = value;
                if ((CuesheetImportFile != null) && (CuesheetImportFile.Cuesheet != null))
                {
                    CuesheetImportFile.AnalysisFinished += CuesheetImportFile_AnalysisFinished;
                    //TODO
                    //ImportCuesheet = CuesheetImportFile.Cuesheet;
                }
                else
                {
                    ImportCuesheet = null;
                }
            }
        }

        public Audiofile? ImportAudiofile
        {
            get => importAudiofile;
            set
            {
                importAudiofile = value;
                if ((ImportCuesheet != null) && (ImportAudiofile != null))
                {
                    ImportCuesheet.Audiofile = ImportAudiofile;
                    ImportCuesheetChanged?.Invoke(this, EventArgs.Empty);
                }
            }
        }

        public ViewMode CurrentViewMode 
        {
            get { return currentViewMode; }
            set
            {
                currentViewMode = value;
                CurrentViewModeChanged?.Invoke(this, EventArgs.Empty);
            }
        }

        public IImportfile? Importfile
        {
            get
            {
                if (TextImportFile != null)
                {
                    return TextImportFile;
                }
                if (CuesheetImportFile != null)
                {
                    return CuesheetImportFile;
                }
                return null;
            }
        }

        public void ResetImport()
        {
            TextImportFile = null;
            CuesheetImportFile = null;
            ImportAudiofile = null;
        }

        public void StartImportCuesheet(ApplicationOptions applicationOptions)
        {
            if (ImportCuesheet != null)
            {
                //TODO
                //Cuesheet.Import(ImportCuesheet, applicationOptions, _traceChangeManager);
                ImportCuesheet = null;
            }
            ResetImport();
        }

        private void SetCuesheetReference(Cuesheet value)
        {
            cuesheet.CuesheetImported -= Cuesheet_CuesheetImported;
            cuesheet = value;
            cuesheet.CuesheetImported += Cuesheet_CuesheetImported;
            _traceChangeManager.Reset();
            _traceChangeManager.TraceChanges(Cuesheet);
            CuesheetChanged?.Invoke(this, EventArgs.Empty);
        }
        
        private void Cuesheet_CuesheetImported(object? sender, EventArgs e)
        {
            CuesheetChanged?.Invoke(this, EventArgs.Empty);
        }

        void CuesheetImportFile_AnalysisFinished(object? sender, EventArgs e)
        {
            if (CuesheetImportFile != null)
            {
                //TODO
                //ImportCuesheet = CuesheetImportFile.Cuesheet;
            }
            else
            {
                ImportCuesheet = null;
            }
        }
    }
}
