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
using AudioCuesheetEditor.Model.Options;
using AudioCuesheetEditor.Services.Options;

namespace AudioCuesheetEditor.Services.UI
{
    public class SessionStateContainer : ISessionStateContainer, IDisposable
    {
        private readonly ILocalStorageOptionsProvider _localStorageOptionsProvider;

        private Cuesheet _cuesheet = new();
        private Cuesheet? _importCuesheet;
        private Cuesheet? _activeCuesheet;
        private ViewOptions? _viewOptions;
        private bool disposedValue;

        public event EventHandler? ActiveCuesheetChanged;

        public SessionStateContainer(ILocalStorageOptionsProvider localStorageOptionsProvider)
        {
            _localStorageOptionsProvider = localStorageOptionsProvider;
            _localStorageOptionsProvider.OptionSaved += LocalStorageOptionsProvider_OptionSaved;
        }

        public void Dispose()
        {
            Dispose(disposing: true);
            GC.SuppressFinalize(this);
        }

        public Cuesheet Cuesheet 
        {
            get => _cuesheet;
            set
            {
                _cuesheet = value;
                SetActiveCuesheet();
            }
        }
        public Cuesheet? ImportCuesheet 
        {
            get => _importCuesheet;
            set
            {
                _importCuesheet = value;
                SetActiveCuesheet();
            }
        }
        public IList<Audiofile> ImportAudiofiles { get; set; } = [];
        public IImportfile? Importfile { get; set; }
        public Boolean ImportIsAnalyzed { get; set; } = false;

        /// <inheritdoc/>
        public async Task InitializeAsync()
        {
            _viewOptions ??= await _localStorageOptionsProvider.GetOptionsAsync<ViewOptions>();
            SetActiveCuesheet();
        }

        public void ResetImport()
        {
            Importfile = null;
            ImportAudiofiles = [];
            ImportCuesheet = null;
            ImportIsAnalyzed = false;
        }

        /// <inheritdoc/>
        public Cuesheet? ActiveCuesheet => _activeCuesheet;

        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing)
                {
                    _localStorageOptionsProvider.OptionSaved -= LocalStorageOptionsProvider_OptionSaved;
                }
                disposedValue = true;
            }
        }

        void LocalStorageOptionsProvider_OptionSaved(object? sender, IOptions options)
        {
            if (options is ViewOptions viewOptions)
            {
                _viewOptions = viewOptions;
                SetActiveCuesheet();
            }
        }

        void SetActiveCuesheet()
        {
            if (_viewOptions == null)
            {
                throw new InvalidOperationException("Not initialized!");
            }
            if (_viewOptions.ActiveTab == ViewMode.ImportView)
            {
                if (_activeCuesheet != ImportCuesheet)
                {
                    _activeCuesheet = ImportCuesheet;
                    ActiveCuesheetChanged?.Invoke(this, EventArgs.Empty);
                }
            }
            else
            {
                if (_activeCuesheet != Cuesheet)
                {
                    _activeCuesheet = Cuesheet;
                    ActiveCuesheetChanged?.Invoke(this, EventArgs.Empty);
                }
            }
        }
    }
}
