using Microsoft.Playwright;

namespace AudioCuesheetEditor.End2EndTests.Models
{
    internal class ImportView(IPage page)
    {
        internal const string BaseUrl = "http://localhost:5132/";

        private readonly IPage _page = page;

        internal async Task GotoAsync()
        {
            await _page.GotoAsync(BaseUrl);
            await _page.WaitForURLAsync(BaseUrl);
            await _page.WaitForLoadStateAsync(LoadState.NetworkIdle);
            await _page.GetByText("Import view").ClickAsync();
        }

        internal async Task ImportFileAsync(string filepath)
        {
            await _page.GetByRole(AriaRole.Button, new() { Name = "Choose File" }).SetInputFilesAsync(filepath);
            await _page.GetByRole(AriaRole.Button, new() { Name = "Complete" }).ClickAsync();
        }
    }
}
