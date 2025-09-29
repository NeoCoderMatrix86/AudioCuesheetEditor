using Microsoft.Playwright;
using System.Text.RegularExpressions;

namespace AudioCuesheetEditor.End2EndTests.Models
{
    internal class AppBar
    {
        internal const string BaseUrl = "http://localhost:5132/";

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

        internal async Task GotoAsync()
        {
            await _page.GotoAsync(BaseUrl);
            await _page.WaitForURLAsync(BaseUrl);
            await _page.WaitForLoadStateAsync(LoadState.NetworkIdle);
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
    }
}
