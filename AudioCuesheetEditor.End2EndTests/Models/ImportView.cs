using Microsoft.Playwright;
using Microsoft.VisualStudio.TestPlatform.ObjectModel;

namespace AudioCuesheetEditor.End2EndTests.Models
{
    internal class ImportView(IPage page)
    {
        internal const string BaseUrl = "http://localhost:5132/";

        private readonly IPage _page = page;

        internal ILocator CompleteButton => _page.GetByRole(AriaRole.Button, new() { Name = "Complete" });

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
        }

        internal async Task CompleteImportAsync()
        {
            await _page.GetByRole(AriaRole.Button, new() { Name = "Complete" }).ClickAsync();
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

        internal async Task EditTracksModalAsync(string title)
        {
            await _page.GetByRole(AriaRole.Button, new() { Name = "Edit selected tracks" }).ClickAsync();
            await _page.GetByRole(AriaRole.Textbox, new() { Name = "Title", Exact = true }).FillAsync(title);
            await _page.GetByRole(AriaRole.Textbox, new() { Name = "Title", Exact = true }).PressAsync("Tab");
            await _page.GetByRole(AriaRole.Button, new() { Name = "Save changes" }).ClickAsync();
        }

        internal async Task SwitchImportProfileAsync(string profile)
        {
            await _page.GetByText("Textfile (common data in").ClickAsync();
            await _page.GetByText(profile).ClickAsync();
        }
    }
}
