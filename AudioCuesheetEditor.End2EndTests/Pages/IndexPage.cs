using Microsoft.Playwright;
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
    }
}
