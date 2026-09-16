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
    partial class AppBar(IPage page)
    {
        private readonly IPage _page = page;
        internal ILocator MenuButton => _page.GetByRole(AriaRole.Toolbar).GetByRole(AriaRole.Button, new() { Name = "More" });

        internal ILocator UndoButton => _page.GetByRole(AriaRole.Button, new() { Name = "undo" });

        internal ILocator RedoButton => _page.GetByRole(AriaRole.Button, new() { Name = "redo" });

        internal ILocator HomeButton => _page.Locator(".mud-button-root").First;

        internal async Task OpenSettingsAsync()
        {
            await MenuButton.ClickAsync();
            await _page.GetByText("Settings").ClickAsync();
        }

        internal async Task ChangeLanguageAsync(string language)
        {
            await _page.GetByRole(AriaRole.Button, new() { Name = "Change language" }).ClickAsync();
            await _page.GetByText(language).ClickAsync();
            await _page.WaitForLoadStateAsync(LoadState.NetworkIdle);
            await _page.WaitForFunctionAsync(@"() => window.Blazor !== undefined");
        }

        internal async Task UndoAsync()
        {
            await UndoButton.ClickAsync();
        }

        internal async Task RedoAsync()
        {
            await RedoButton.ClickAsync();
        }

        internal async Task OpenFileAsync(string file)
        {
            await OpenFileDialogAsync();
            await _page.GetByLabel("Open file upload").SetInputFilesAsync(file);
        }

        internal async Task OpenFileDialogAsync()
        {
            await _page.GetByRole(AriaRole.Toolbar).GetByRole(AriaRole.Button, new() { Name = "More" }).ClickAsync();
            await _page.GetByRole(AriaRole.Menuitem, new() { Name = "Open file" }).ClickAsync();
        }

        internal async Task OpenExportDialogAsync(string exportType)
        {
            await _page.GetByRole(AriaRole.Button, new() { Name = "Export menu" }).ClickAsync();
            await _page.GetByText(exportType, new() { Exact = true }).ClickAsync();
        }

        internal async Task OpenDisplayHotkeysAsync()
        {
            await MenuButton.ClickAsync();
            await _page.GetByText("Hotkeys").ClickAsync();
        }
    }
}
