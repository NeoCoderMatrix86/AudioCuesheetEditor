using Microsoft.Playwright;

namespace AudioCuesheetEditor.End2EndTests.Models
{
    internal class About(IPage page)
    {
        internal const string BaseUrl = "http://localhost:5132/about";

        private readonly IPage _page = page;

        internal ILocator AboutHeading => _page.GetByRole(AriaRole.Heading, new() { Name = "About AudioCuesheetEditor" });

        internal async Task GotoAsync()
        {
            await _page.GotoAsync(BaseUrl);
            await _page.WaitForURLAsync(BaseUrl);
            await _page.WaitForLoadStateAsync(LoadState.NetworkIdle);
        }
    }
}
