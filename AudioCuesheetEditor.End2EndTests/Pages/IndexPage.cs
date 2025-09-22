using Microsoft.Playwright;
using Microsoft.VisualStudio.TestPlatform.ObjectModel;
using System.Text.RegularExpressions;

namespace AudioCuesheetEditor.End2EndTests.Pages
{
    public class IndexPage
    {
        public const string BaseUrl = "http://localhost:5132/";

        private readonly IPage _page;
        private readonly ILocator _menuButton;

        public IndexPage(IPage page)
        {
            _page = page;
            _menuButton = _page.GetByRole(AriaRole.Toolbar)
                .GetByRole(AriaRole.Button)
                .Filter(new() { HasTextRegex = new Regex("^$") })
                .Nth(3);
        }

        public async Task GotoAsync()
        {
            await _page.GotoAsync(BaseUrl);
            await _page.WaitForURLAsync(BaseUrl);
            await _page.WaitForLoadStateAsync(LoadState.NetworkIdle);
        }

        public async Task OpenSettingsAsync()
        {
            await _menuButton.ClickAsync();
            await _page.GetByText("Settings").ClickAsync();
        }

        public async Task StartRecordingAsync()
        {
            await _page.GetByText("Record view").ClickAsync();
            await _page.GetByRole(AriaRole.Button, new() { Name = "Start recording" }).ClickAsync();
        }

        public async Task StopRecordingAsync()
        {
            await _page.GetByRole(AriaRole.Button, new() { Name = "Stop recording" }).ClickAsync();
        }

        public async Task AddRecordingTrackAsync(string artist, string title)
        {
            await _page.GetByRole(AriaRole.Textbox, new() { Name = "Artist", Exact = true }).ClickAsync();
            await _page.GetByRole(AriaRole.Textbox, new() { Name = "Artist", Exact = true }).FillAsync(artist);
            await _page.GetByRole(AriaRole.Textbox, new() { Name = "Artist", Exact = true }).PressAsync("Tab");
            await _page.GetByRole(AriaRole.Textbox, new() { Name = "Title", Exact = true }).FillAsync(title);
            await _page.GetByRole(AriaRole.Textbox, new() { Name = "Title", Exact = true }).PressAsync("Tab");
            await _page.GetByRole(AriaRole.Button, new() { Name = "Add track" }).ClickAsync();
            await _page.Locator(".mud-overlay").ClickAsync();
        }
    }
}
