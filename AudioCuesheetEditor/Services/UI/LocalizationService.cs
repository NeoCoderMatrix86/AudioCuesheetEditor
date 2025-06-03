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
using System.Globalization;

namespace AudioCuesheetEditor.Services.UI
{
    public class LocalizationService(ILocalStorageOptionsProvider localStorageOptionsProvider)
    {
        private readonly ILocalStorageOptionsProvider _localStorageOptionsProvider = localStorageOptionsProvider;

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

        public event EventHandler? LocalizationChanged;

        public static CultureInfo SelectedCulture => CultureInfo.DefaultThreadCurrentUICulture ?? CultureInfo.CurrentUICulture;

        public async Task SetCultureFromConfigurationAsync()
        {
            var options = await _localStorageOptionsProvider.GetOptions<ApplicationOptions>();

            ChangeLanguage(options.Culture.Name);
        }

        public async Task ChangeLanguageAsync(string name)
        {
            if (ChangeLanguage(name))
            {
                await _localStorageOptionsProvider.SaveOptionsValue<ApplicationOptions>(x => x.CultureName!, name);
                LocalizationChanged?.Invoke(this, new EventArgs());
            }
        }

        private static Boolean ChangeLanguage(string name)
        {
            var newCulture = AvailableCultures.SingleOrDefault(c => c.Name == name);
            if (newCulture != null)
            {
                CultureInfo.DefaultThreadCurrentUICulture = newCulture;
                CultureInfo.CurrentUICulture = newCulture;
            }
            return newCulture != null;
        }
    }
}
