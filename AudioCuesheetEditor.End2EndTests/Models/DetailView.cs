using Microsoft.Playwright;

namespace AudioCuesheetEditor.End2EndTests.Models
{
    internal class DetailView(IPage page, bool mobile)
    {
        internal const string BaseUrl = "http://localhost:5132/";

        private readonly IPage _page = page;
        private readonly bool _isMobile = mobile;

        internal ILocator AudiofileInput => _page.GetByRole(AriaRole.Group).Filter(new() { HasText = "AudiofileAudiofile" }).Locator("input[type=\"file\"]");

        internal ILocator CuesheetArtistInput => _page.GetByRole(AriaRole.Textbox, new() { Name = "Cuesheet artist" });

        internal ILocator CuesheetTitleInput => _page.GetByRole(AriaRole.Textbox, new() { Name = "Cuesheet title" });

        internal async Task GotoAsync()
        {
            await _page.GotoAsync(BaseUrl);
            await _page.WaitForURLAsync(BaseUrl);
            await _page.WaitForLoadStateAsync(LoadState.NetworkIdle);
        }

        internal async Task AddTrackAsync()
        {
            await _page.GetByRole(AriaRole.Button, new() { Name = "Add new track" }).ClickAsync();
        }

        internal async Task EditTrackAsync(string? artist = null, string? title = null)
        {
            if (artist != null)
            {
                await _page.Locator("td:nth-child(3)").ClickAsync();
                await _page.Locator("td:nth-child(3)").Last.GetByRole(AriaRole.Textbox).FillAsync(artist);
                await _page.Keyboard.PressAsync("Escape");
                if (await _page.IsVisibleAsync(".mud-overlay"))
                {
                    await _page.Locator(".mud-overlay").ClickAsync();
                }
                await _page.GetByRole(AriaRole.Heading, new() { Name = "Playback" }).ClickAsync();
                await page.WaitForTimeoutAsync(200);
            }
            if (title != null)
            {
                await _page.Locator("td:nth-child(4)").ClickAsync();
                await _page.Locator("td:nth-child(4)").Last.GetByRole(AriaRole.Textbox).FillAsync(title);
                await _page.Keyboard.PressAsync("Escape");
                if (await _page.IsVisibleAsync(".mud-overlay"))
                {
                    await _page.Locator(".mud-overlay").ClickAsync();
                }
                await _page.GetByRole(AriaRole.Heading, new() { Name = "Playback" }).ClickAsync();
                await page.WaitForTimeoutAsync(200);
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

        internal async Task EditTracksModalAsync(string artist, string title, string end, IEnumerable<string> flagsToSelect)
        {
            await _page.GetByRole(AriaRole.Button, new() { Name = "Edit selected tracks" }).ClickAsync();
            await _page.GetByRole(AriaRole.Textbox, new() { Name = "Artist", Exact = true }).FillAsync(artist);
            await _page.GetByRole(AriaRole.Textbox, new() { Name = "Artist", Exact = true }).PressAsync("Tab");
            await _page.GetByRole(AriaRole.Textbox, new() { Name = "Title", Exact = true }).FillAsync(title);
            await _page.GetByRole(AriaRole.Textbox, new() { Name = "Title", Exact = true }).PressAsync("Tab");
            await _page.GetByRole(AriaRole.Textbox, new() { Name = "End" }).FillAsync(end);
            foreach (var flag in flagsToSelect)
            {
                await _page.GetByRole(AriaRole.Button, new() { Name = flag }).ClickAsync();
            }
            await _page.GetByRole(AriaRole.Button, new() { Name = "Save changes" }).ClickAsync();
        }

        internal async Task RenameAudiofileAsync(string filename)
        {
            int buttonIndex = _isMobile ? 2 : 3;
            await _page.GetByRole(AriaRole.Group).Filter(new() { HasText = "AudiofileAudiofile" }).GetByRole(AriaRole.Button).Nth(buttonIndex).ClickAsync();
            await _page.GetByText("Rename file").ClickAsync();
            await _page.GetByRole(AriaRole.Textbox, new() { Name = "New file name" }).FillAsync(filename);
            await _page.GetByRole(AriaRole.Button, new() { Name = "Ok" }).ClickAsync();
        }
    }
}
