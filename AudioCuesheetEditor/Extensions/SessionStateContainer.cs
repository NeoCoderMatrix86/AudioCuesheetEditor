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
        //TODO: Maybe use importcuesheet class?
        public Cuesheet? ImportCuesheet
        {
            get { return importCuesheet; }
            set
            {
                importCuesheet = value;
                ImportCuesheetChanged?.Invoke(this, EventArgs.Empty);
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
    }
}
