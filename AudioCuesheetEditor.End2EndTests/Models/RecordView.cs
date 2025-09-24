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
        }

        internal async Task StartRecordingAsync()
        {
            await _page.GetByText("Record view").ClickAsync();
            await _page.GetByRole(AriaRole.Button, new() { Name = "Start recording" }).ClickAsync();
        }

        internal async Task StopRecordingAsync()
        {
            await _page.GetByRole(AriaRole.Button, new() { Name = "Stop recording" }).ClickAsync();
        }

        internal async Task AddRecordingTrackAsync(string artist, string title)
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
