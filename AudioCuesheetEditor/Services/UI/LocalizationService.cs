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
using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;
using System.Globalization;

namespace AudioCuesheetEditor.Services.UI
{
    public class LocalizationService(ILocalStorageOptionsProvider localStorageOptionsProvider, NavigationManager navigationManager, IJSRuntime jsRuntime)
    {
        private readonly ILocalStorageOptionsProvider _localStorageOptionsProvider = localStorageOptionsProvider;
        private readonly NavigationManager _navigationManager = navigationManager;
        private readonly IJSRuntime _jsRuntime = jsRuntime;

        public const String DefaultCulture = "en-US";

        public static IReadOnlyCollection<CultureInfo> AvailableCultures
        {
            get
            {
                var cultures = new List<CultureInfo>
                {
                    new(DefaultCulture),
                    new("de-DE")
                };
                return cultures.AsReadOnly();
            }
        }

        public static CultureInfo SelectedCulture => CultureInfo.DefaultThreadCurrentUICulture ?? CultureInfo.CurrentUICulture;

        public async Task SetCultureFromConfigurationAsync()
        {
            var options = await _localStorageOptionsProvider.GetOptionsAsync<ApplicationOptions>();

            ChangeLanguage(options.Culture);
        }

        public async Task ChangeLanguageAsync(CultureInfo culture)
        {
            if (ChangeLanguage(culture))
            {
                await _localStorageOptionsProvider.SaveOptionsValueAsync<ApplicationOptions>(x => x.CultureName!, culture.Name);
                await _jsRuntime.InvokeVoidAsync("removeBeforeunload");
                _navigationManager.NavigateTo(_navigationManager.Uri, true);
            }
        }

        private static Boolean ChangeLanguage(CultureInfo newCulture)
        {
            var contains = AvailableCultures.Contains(newCulture);
            if (contains)
            {
                CultureInfo.DefaultThreadCurrentUICulture = newCulture;
                CultureInfo.DefaultThreadCurrentCulture = newCulture;
            }
            return contains;
        }
    }
}
