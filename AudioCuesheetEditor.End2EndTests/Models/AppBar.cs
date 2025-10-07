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
    internal class AppBar
    {
        private readonly IPage _page;
        private readonly ILocator _menuButton;

        internal ILocator UndoButton => _page.GetByRole(AriaRole.Button, new() { Name = "undo" });

        internal ILocator RedoButton => _page.GetByRole(AriaRole.Button, new() { Name = "redo" });

        internal ILocator HomeButton => _page.Locator(".mud-button-root").First;

        internal AppBar(IPage page)
        {
            _page = page;
            _menuButton = _page.GetByRole(AriaRole.Toolbar)
                .GetByRole(AriaRole.Button)
                .Filter(new() { HasTextRegex = new Regex("^$") })
                .Nth(3);
        }

        internal async Task OpenSettingsAsync()
        {
            await _menuButton.ClickAsync();
            await _page.GetByText("Settings").ClickAsync();
        }

        internal async Task ChangeLanguageAsync(string language)
        {
            await _page.GetByRole(AriaRole.Button, new() { Name = "Change language" }).ClickAsync();
            await _page.GetByText(language).ClickAsync();
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
            await _page.GetByRole(AriaRole.Button, new() { Name = "File", Exact = true }).ClickAsync();
            await _page.Locator("div").Filter(new() { HasTextRegex = new Regex("^Open$") }).ClickAsync();
            await _page.Locator("#dropFileInputId_SelectFileDialog").GetByRole(AriaRole.Button, new() { Name = "Choose File" }).ClickAsync();
            await _page.Locator("#dropFileInputId_SelectFileDialog").GetByRole(AriaRole.Button, new() { Name = "Choose File" }).SetInputFilesAsync(file);
        }

        internal async Task OpenExportDialogAsync(string exportType, string fileMenuName = "File")
        {
            await _page.GetByRole(AriaRole.Button, new() { Name = fileMenuName, Exact = true }).ClickAsync();
            await _page.GetByText("Export", new() { Exact = true }).HoverAsync();
            await _page.GetByText(exportType, new() { Exact = true }).ClickAsync();
        }
    }
}
