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
using AudioCuesheetEditor.Extensions;

namespace AudioCuesheetEditor.Services.IO
{
    public class ImportManager : IDisposable
    {
        private readonly SessionStateContainer _sessionStateContainer;
        private bool disposedValue;

        public event EventHandler? FileContentChanged;

        public ImportManager(SessionStateContainer sessionStateContainer)
        {
            _sessionStateContainer = sessionStateContainer;
            _sessionStateContainer.ImportCuesheetChanged += SessionStateContainer_ImportCuesheetChanged;
        }

        public IEnumerable<String?>? FileContentRecognized
        {
            get
            {
                if (_sessionStateContainer.TextImportFile != null)
                {
                    return _sessionStateContainer.TextImportFile.FileContentRecognized;
                }
                if (_sessionStateContainer.CuesheetImportFile != null)
                {
                    return _sessionStateContainer.CuesheetImportFile.FileContentRecognized;
                }
                return null;
            }
        }

        public String? FileContent
        {
            get
            {
                if (_sessionStateContainer.TextImportFile?.FileContent != null)
                {
                    return String.Join(Environment.NewLine, _sessionStateContainer.TextImportFile.FileContent);
                }
                if (_sessionStateContainer.CuesheetImportFile?.FileContent != null)
                {
                    return String.Join(Environment.NewLine, _sessionStateContainer.CuesheetImportFile.FileContent);
                }
                return null;
            }
            set
            {
                var fileContentValue = value?.Split(Environment.NewLine);
                if (fileContentValue != null)
                {
                    if (_sessionStateContainer.TextImportFile != null)
                    {
                        _sessionStateContainer.TextImportFile.FileContent = fileContentValue;
                    }
                    if (_sessionStateContainer.CuesheetImportFile != null)
                    {
                        _sessionStateContainer.CuesheetImportFile.FileContent = fileContentValue;
                    }
                }
            }
        }

        public void Dispose()
        {
            // Ändern Sie diesen Code nicht. Fügen Sie Bereinigungscode in der Methode "Dispose(bool disposing)" ein.
            Dispose(disposing: true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing)
                {
                    _sessionStateContainer.ImportCuesheetChanged -= SessionStateContainer_ImportCuesheetChanged;
                }

                disposedValue = true;
            }
        }

        private void SessionStateContainer_ImportCuesheetChanged(object? sender, EventArgs e)
        {
            FileContentChanged?.Invoke(this, e);
        }
    }
}
