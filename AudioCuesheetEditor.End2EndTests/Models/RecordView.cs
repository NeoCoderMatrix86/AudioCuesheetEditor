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
    internal class RecordView(IPage page)
    {
        internal const string BaseUrl = "http://localhost:5132/";

        private readonly IPage _page = page;

        internal async Task GotoAsync()
        {
            await _page.GotoAsync(BaseUrl);
            await _page.WaitForURLAsync(BaseUrl);
            await _page.WaitForLoadStateAsync(LoadState.NetworkIdle);
            await _page.WaitForFunctionAsync(@"() => window.Blazor !== undefined");
            await _page.GetByText("Record view").ClickAsync();
        }

        internal async Task StartRecordingAsync()
        {
            await _page.GetByRole(AriaRole.Button, new() { Name = "Start recording" }).ClickAsync();
        }

        internal async Task StopRecordingAsync()
        {
            await _page.GetByRole(AriaRole.Button, new() { Name = "Stop recording" }).ClickAsync();
        }

        internal async Task AddRecordingTrackAsync(string artist, string title)
        {
            await _page.GetByRole(AriaRole.Combobox, new() { Name = "Artist" }).ClickAsync();
            await _page.GetByRole(AriaRole.Combobox, new() { Name = "Artist" }).FillAsync(artist);
            await _page.GetByRole(AriaRole.Combobox, new() { Name = "Artist" }).PressAsync("Tab");
            await _page.GetByRole(AriaRole.Combobox, new() { Name = "Title" }).FillAsync(title);
            await _page.GetByRole(AriaRole.Combobox, new() { Name = "Title" }).PressAsync("Tab");
            await _page.GetByRole(AriaRole.Button, new() { Name = "Add track" }).ClickAsync();
        }
    }
}
