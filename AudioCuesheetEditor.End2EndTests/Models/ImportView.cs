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
using Microsoft.Playwright;
using System.Text.RegularExpressions;

namespace AudioCuesheetEditor.End2EndTests.Models
{
    partial class ImportView(IPage page)
    {
        [GeneratedRegex("^Scheme common data$")]
        private static partial Regex SchemeCommonData();

        internal const string BaseUrl = "http://localhost:5132/";

        private readonly IPage _page = page;

        internal ILocator CuesheetArtistInput => _page.GetByRole(AriaRole.Textbox, new() { Name = "Cuesheet artist" });

        internal ILocator CuesheetTitleInput => _page.GetByRole(AriaRole.Textbox, new() { Name = "Cuesheet title" });

        internal ILocator CatalogueNumberInput => _page.GetByRole(AriaRole.Textbox, new() { Name = "Cataloguenumber" });

        internal async Task GotoAsync()
        {
            await _page.GotoAsync(BaseUrl);
            await _page.WaitForURLAsync(BaseUrl);
            await _page.WaitForLoadStateAsync(LoadState.NetworkIdle);
            await _page.GetByText("Import view").ClickAsync();
        }

        internal async Task ImportFileAsync(string filepath)
        {
            await _page.GetByRole(AriaRole.Button, new() { Name = "Choose File" }).SetInputFilesAsync(filepath);
        }

        internal async Task CompleteImportAsync()
        {
            await _page.GetByRole(AriaRole.Button, new() { Name = "Complete" }).ClickAsync();
        }

        internal async Task SelectTracksAsync(IEnumerable<int> trackTablePositions, Boolean uncheck = false)
        {
            foreach (var trackTablePosition in trackTablePositions)
            {
                if (uncheck)
                {
                    await _page.Locator($"tr:nth-child({trackTablePosition + 1}) > td").First.GetByRole(AriaRole.Checkbox).UncheckAsync();
                }
                else
                {
                    await _page.Locator($"tr:nth-child({trackTablePosition + 1}) > td").First.GetByRole(AriaRole.Checkbox).CheckAsync();
                }
            }
        }

        internal async Task EditTracksModalAsync(string title)
        {
            await _page.GetByRole(AriaRole.Button, new() { Name = "Edit selected tracks" }).ClickAsync();
            await _page.GetByRole(AriaRole.Textbox, new() { Name = "Title", Exact = true }).FillAsync(title);
            await _page.GetByRole(AriaRole.Textbox, new() { Name = "Title", Exact = true }).PressAsync("Tab");
            await _page.GetByRole(AriaRole.Button, new() { Name = "Save changes" }).ClickAsync();
        }

        internal async Task SwitchImportProfileAsync(string profile)
        {
            await _page.GetByText("Textfile (common data in").ClickAsync();
            await _page.GetByText(profile).ClickAsync();
        }

        internal async Task ClearSchemeCommonDataAsync()
        {
            await _page.Locator("div").Filter(new() { HasTextRegex = SchemeCommonData() }).GetByLabel("Clear").ClickAsync();
        }

        internal async Task SetSchemeCommonDataAsync(string schemeCommonData)
        {
            await _page.GetByRole(AriaRole.Textbox, new() { Name = "Scheme common data" }).FillAsync(schemeCommonData);
        }

        internal async Task SelectSchemeCommonDataPlaceholderAsync(string placeholder)
        {
            await _page.Locator("div").Filter(new() { HasTextRegex = SchemeCommonData() }).GetByRole(AriaRole.Button).Nth(1).ClickAsync();
            await _page.GetByRole(AriaRole.Paragraph).Filter(new() { HasText = placeholder }).ClickAsync();
        }
    }
}
