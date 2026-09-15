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

namespace AudioCuesheetEditor.End2EndTests.Models
{
    internal class DetailView(IPage page)
    {
        internal const string BaseUrl = "http://localhost:5132/";

        private readonly IPage _page = page;

        internal ILocator CuesheetArtistInput => _page.GetByRole(AriaRole.Textbox, new() { Name = "Cuesheet artist" });

        internal ILocator CuesheetTitleInput => _page.GetByRole(AriaRole.Textbox, new() { Name = "Cuesheet title" });

        internal ILocator NewFileNameInput => _page.GetByRole(AriaRole.Textbox, new() { Name = "New file name" });

        internal async Task GotoAsync()
        {
            await _page.GotoAsync(BaseUrl);
            await _page.WaitForURLAsync(BaseUrl);
            await _page.WaitForLoadStateAsync(LoadState.NetworkIdle);
            await _page.WaitForFunctionAsync(@"() => window.Blazor !== undefined");
        }

        internal async Task AddAudiofileAsync()
        {
            await _page.GetByRole(AriaRole.Button, new() { Name = "Add file" }).ClickAsync();
        }

        internal async Task SetAudiofileInputFileAsync(int audiofileIndex, string file)
        {
            await _page.GetByRole(AriaRole.Group).Filter(new() { HasText = "AudiofileAudiofile" }).Nth(audiofileIndex).Locator("input[type=\"file\"]").SetInputFilesAsync(file);
        }

        internal async Task AddTrackAsync(int audiofileIndex)
        {
            await _page.GetByRole(AriaRole.Button, new() { Name = "Add new track" }).Nth(audiofileIndex).ClickAsync();
        }

        internal async Task EditTrackAsync(string? artist = null, string? title = null, TimeSpan? begin = null, TimeSpan? end = null, int trackPosition = 1)
        {
            if (artist != null)
            {
                await EditTrackFieldAsync(trackPosition, "Artist", artist);
            }
            if (title != null)
            {
                await EditTrackFieldAsync(trackPosition, "Title", title);
            }
            if (begin != null)
            {
                await EditTrackFieldAsync(trackPosition, "Begin", begin.ToString() ?? string.Empty);
            }
            if (end != null)
            {
                await EditTrackFieldAsync(trackPosition, "End", end.ToString() ?? string.Empty);
            }
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

        internal async Task EditSelectedTracksModalAsync()
        {
            await _page.GetByRole(AriaRole.Button, new() { Name = "Edit selected tracks" }).ClickAsync();
        }

        internal async Task EditTracksModalAsync(string artist, string title, string end, IEnumerable<string> flagsToSelect)
        {
            await _page.GetByRole(AriaRole.Button, new() { Name = "Edit selected tracks" }).ClickAsync();
            await _page.GetByRole(AriaRole.Combobox, new() { Name = "Artist" }).FillAsync(artist);
            await _page.GetByRole(AriaRole.Combobox, new() { Name = "Artist" }).PressAsync("Tab");
            await _page.GetByRole(AriaRole.Combobox, new() { Name = "Title" }).FillAsync(title);
            await _page.GetByRole(AriaRole.Combobox, new() { Name = "Title" }).PressAsync("Tab");
            await _page.GetByRole(AriaRole.Textbox, new() { Name = "End" }).FillAsync(end);
            foreach (var flag in flagsToSelect)
            {
                await _page.GetByRole(AriaRole.Button, new() { Name = flag }).ClickAsync();
            }
            await _page.GetByRole(AriaRole.Button, new() { Name = "Save changes" }).ClickAsync();
        }

        internal async Task RenameAudiofileAsync(int audiofileIndex, string filename)
        {
            await OpenRenameAudiofileDialogAsync(audiofileIndex);
            await NewFileNameInput.FillAsync(filename);
            await _page.GetByRole(AriaRole.Button, new() { Name = "Ok" }).ClickAsync();
        }

        internal async Task OpenRenameAudiofileDialogAsync(int audiofileIndex)
        {
            await _page.GetByRole(AriaRole.Group).Filter(new() { HasText = "AudiofileAudiofile" }).Nth(audiofileIndex).GetByLabel("More").ClickAsync();
            await _page.GetByText("Rename file").ClickAsync();
        }

        async Task EditTrackFieldAsync(int trackPosition, string dataLabel, string value)
        {
            var row = _page.Locator("tbody tr:not([aria-hidden='true'])").Nth(trackPosition - 1);
            var cell = row.Locator($"td[data-label='{dataLabel}']");
            await cell.ClickAsync();
            var textbox = cell.Locator("input[type='text']");
            await textbox.FillAsync(value);
            // Click outside the autocomplete to have an focus lost event for getting the value written to model
            await _page.GetByRole(AriaRole.Heading, new() { Name = "Playback" }).ClickAsync(new() { Force = true });
            await _page.WaitForTimeoutAsync(100);
        }
    }
}
