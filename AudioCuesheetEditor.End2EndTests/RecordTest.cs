using AudioCuesheetEditor.End2EndTests.Pages;
using Microsoft.Playwright;

namespace AudioCuesheetEditor.End2EndTests
{
    [TestClass]
    public class RecordTest : PlaywrightTestBase
    {
        [TestMethod]
        public async Task RecordAsync()
        {
            var indexPage = new IndexPage(TestPage);
            await indexPage.GotoAsync();
            await indexPage.StartRecordingAsync();
            await indexPage.AddRecordingTrackAsync("Test Track 1 Artist", "Test Track 1 Title");
            await indexPage.AddRecordingTrackAsync("Test Track 2 Artist", "Test Track 2 Title");
            await indexPage.StopRecordingAsync();
            await Expect(TestPage.GetByRole(AriaRole.Cell, new() { Name = "Test Track 1 Artist Clear" })).ToBeVisibleAsync();
            await Expect(TestPage.GetByRole(AriaRole.Cell, new() { Name = "Test Track 1 Title Clear" })).ToBeVisibleAsync();
            await Expect(TestPage.GetByRole(AriaRole.Cell, new() { Name = "Test Track 2 Artist Clear" })).ToBeVisibleAsync();
            await Expect(TestPage.GetByRole(AriaRole.Cell, new() { Name = "Test Track 2 Title Clear" })).ToBeVisibleAsync();
        }
    }
}
