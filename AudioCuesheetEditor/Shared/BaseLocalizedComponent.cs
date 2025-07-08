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
using AudioCuesheetEditor.Services.UI;
using Microsoft.AspNetCore.Components;

namespace AudioCuesheetEditor.Shared
{
    public abstract class BaseLocalizedComponent : ComponentBase, IDisposable
    {
        private bool disposedValue;

        [Inject]
        protected LocalizationService LocalizationService { get; set; } = default!;
        [Inject]
        protected ITraceChangeManager TraceChangeManager { get; set; } = default!;
        [Inject]
        protected ILocalStorageOptionsProvider LocalStorageOptionsProvider { get; set; } = default!;
        //TODO: Remove this, since a rerendering costs performance
        public ApplicationOptions? ApplicationOptions { get; private set; }

        protected override void OnInitialized()
        {
            base.OnInitialized();
            LocalizationService.LocalizationChanged += LocalizationService_LocalizationChanged;
            TraceChangeManager.TracedObjectHistoryChanged += TraceChangeManager_TracedObjectHistoryChanged;
            TraceChangeManager.UndoDone += TraceChangeManager_UndoDone;
            TraceChangeManager.RedoDone += TraceChangeManager_RedoDone;
        }

        protected override async Task OnInitializedAsync()
        {
            await base.OnInitializedAsync();
            ApplicationOptions = await LocalStorageOptionsProvider.GetOptionsAsync<ApplicationOptions>();
            LocalStorageOptionsProvider.OptionSaved += LocalStorageOptionsProvider_OptionSaved;
        }

        void LocalStorageOptionsProvider_OptionSaved(object? sender, IOptions option)
        {
            if (option is ApplicationOptions applicationOptions)
            {
                ApplicationOptions = applicationOptions;
                StateHasChanged();
            }
        }

        void TraceChangeManager_RedoDone(object? sender, EventArgs e)
        {
            StateHasChanged();
        }

        void TraceChangeManager_UndoDone(object? sender, EventArgs e)
        {
            StateHasChanged();
        }

        void TraceChangeManager_TracedObjectHistoryChanged(object? sender, EventArgs e)
        {
            StateHasChanged();
        }

        void LocalizationService_LocalizationChanged(object? sender, EventArgs args)
        {
            StateHasChanged();
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing)
                {
                    LocalizationService.LocalizationChanged -= LocalizationService_LocalizationChanged;
                    TraceChangeManager.UndoDone -= TraceChangeManager_UndoDone;
                    TraceChangeManager.RedoDone -= TraceChangeManager_RedoDone;
                    LocalStorageOptionsProvider.OptionSaved -= LocalStorageOptionsProvider_OptionSaved;
                }

                disposedValue = true;
            }
        }

        public void Dispose()
        {
            // Ändern Sie diesen Code nicht. Fügen Sie Bereinigungscode in der Methode "Dispose(bool disposing)" ein.
            Dispose(disposing: true);
            GC.SuppressFinalize(this);
        }
    }
}