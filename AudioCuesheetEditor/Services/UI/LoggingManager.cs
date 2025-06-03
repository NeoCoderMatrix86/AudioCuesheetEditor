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
using AudioCuesheetEditor.Data.Options;
using AudioCuesheetEditor.Model.Options;

namespace AudioCuesheetEditor.Services.UI
{
    public class LoggingManager : IDisposable
    {
        public const LogLevel DefaultLogLevel = LogLevel.Information;
        private readonly ILocalStorageOptionsProvider _localStorageOptionsProvider;
        private ApplicationOptions? applicationOptions;
        private bool disposedValue;

        public LoggingManager(ILocalStorageOptionsProvider localStorageOptionsProvider)
        {
            _localStorageOptionsProvider = localStorageOptionsProvider;
            _localStorageOptionsProvider.OptionSaved += LocalStorageOptionsProvider_OptionSaved;
            Task.Run(InitAsync);
        }

        public void Dispose()
        {
            // Ändern Sie diesen Code nicht. Fügen Sie Bereinigungscode in der Methode "Dispose(bool disposing)" ein.
            Dispose(disposing: true);
            GC.SuppressFinalize(this);
        }

        public LogLevel LogLevel => applicationOptions != null ? applicationOptions.MinimumLogLevel : DefaultLogLevel;
        
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

        private async Task InitAsync()
        {
            applicationOptions ??= await _localStorageOptionsProvider.GetOptionsAsync<ApplicationOptions>();
        }

        private void LocalStorageOptionsProvider_OptionSaved(object? sender, IOptions option)
        {
            if (option is ApplicationOptions options)
            {
                applicationOptions = options;
            }
        }        
    }
}
