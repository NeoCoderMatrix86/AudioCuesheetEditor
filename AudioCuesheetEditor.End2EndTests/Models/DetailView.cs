using Microsoft.Playwright;

namespace AudioCuesheetEditor.End2EndTests.Models
{
    public class DetailView(IPage page)
    {
        public const string BaseUrl = "http://localhost:5132/";

        private readonly IPage _page = page;

        public async Task GotoAsync()
        {
            await _page.GotoAsync(BaseUrl);
            await _page.WaitForURLAsync(BaseUrl);
            await _page.WaitForLoadStateAsync(LoadState.NetworkIdle);
        }

        public async Task AddTrackAsync(string artist)
        {
            await _page.GetByRole(AriaRole.Group).Filter(new() { HasText = "Edit selected tracks Copy" }).GetByRole(AriaRole.Button).First.ClickAsync();
            await _page.Locator("td:nth-child(3)").Last.GetByRole(AriaRole.Textbox).FillAsync(artist);
            await _page.Locator(".mud-overlay").ClickAsync();
        }

        public async Task EditTrackAsync(string? artist = null, string? title = null)
        {
            if (artist != null)
            {
                await _page.Locator("td:nth-child(3)").ClickAsync();
                await _page.Locator("td:nth-child(3)").Last.GetByRole(AriaRole.Textbox).FillAsync(artist);
                await _page.Locator(".mud-overlay").ClickAsync();
                await _page.Locator("td:nth-child(3)").PressAsync("Tab");
            }
            if (title != null)
            {
                await _page.Locator("td:nth-child(4)").ClickAsync();
                await _page.Locator("td:nth-child(4)").Last.GetByRole(AriaRole.Textbox).FillAsync(title);
                await _page.Locator(".mud-overlay").ClickAsync();
                await _page.Locator("td:nth-child(4)").PressAsync("Tab");
            }
        }
    }
}
