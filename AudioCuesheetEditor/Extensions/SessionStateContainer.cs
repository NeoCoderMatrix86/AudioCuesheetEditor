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
using AudioCuesheetEditor.Controller;
using AudioCuesheetEditor.Model.AudioCuesheet;
using AudioCuesheetEditor.Model.IO.Audio;
using AudioCuesheetEditor.Model.IO.Import;
using AudioCuesheetEditor.Model.Options;
using AudioCuesheetEditor.Model.UI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

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
        private TextImportfile? textImportFile;
        private CuesheetImportfile? cuesheetImportFile;
        private Audiofile? importAudioFile;

        public SessionStateContainer(TraceChangeManager traceChangeManager)
        {
            _traceChangeManager = traceChangeManager ?? throw new ArgumentNullException(nameof(traceChangeManager));
            cuesheet = new Cuesheet();
            _traceChangeManager.TraceChanges(Cuesheet);
        }
        public Cuesheet Cuesheet 
        {
            get { return cuesheet; }
            set
            {
                cuesheet = value;
                _traceChangeManager.Reset();
                _traceChangeManager.TraceChanges(Cuesheet);
                CuesheetChanged?.Invoke(this, EventArgs.Empty);
            }
        }
        public Cuesheet? ImportCuesheet
        {
            get { return importCuesheet; }
            set
            {
                importCuesheet = value;
                //When there is an audiofile from import, we use this file because it has an object url and gets duration, etc.
                if ((importCuesheet != null) && (ImportAudioFile != null))
                {
                    importCuesheet.Audiofile = ImportAudioFile;
                }
                ImportCuesheetChanged?.Invoke(this, EventArgs.Empty);
            }
        }

        public TextImportfile? TextImportFile
        {
            get { return textImportFile; }
            set
            {
                if (textImportFile != null)
                {
                    textImportFile.TextImportScheme.SchemeChanged -= TextImportScheme_SchemeChanged;
                }
                textImportFile = value;
                if (textImportFile != null)
                {
                    textImportFile.TextImportScheme.SchemeChanged += TextImportScheme_SchemeChanged;
                    ImportCuesheet = textImportFile.Cuesheet;
                }
                else
                {
                    ImportCuesheet = null;
                }
            }
        }

        public CuesheetImportfile? CuesheetImportFile
        {
            get { return cuesheetImportFile; }
            set
            {
                cuesheetImportFile = value;
                if ((CuesheetImportFile != null) && (CuesheetImportFile.Cuesheet != null))
                {
                    ImportCuesheet = CuesheetImportFile.Cuesheet;
                }
                else
                {
                    ImportCuesheet = null;
                }
            }
        }

        public Audiofile? ImportAudioFile
        {
            get => importAudioFile;
            set
            {
                importAudioFile = value;
                if ((ImportCuesheet != null) && (ImportAudioFile != null))
                {
                    ImportCuesheet.Audiofile = ImportAudioFile;
                    ImportCuesheetChanged?.Invoke(this, EventArgs.Empty);
                }
            }
        }

        private void TextImportScheme_SchemeChanged(object? sender, string e)
        {
            if (textImportFile != null)
            {
                ImportCuesheet = textImportFile.Cuesheet;
            }
            else
            {
                ImportCuesheet = null;
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

        public void ResetImport()
        {
            TextImportFile = null;
            CuesheetImportFile = null;
            ImportAudioFile = null;
        }

        public void StartImportCuesheet(ApplicationOptions applicationOptions)
        {
            if (ImportCuesheet != null)
            {
                Cuesheet.Import(ImportCuesheet, applicationOptions);
                ImportCuesheet = null;
            }
            ResetImport();
        }
    }
}
