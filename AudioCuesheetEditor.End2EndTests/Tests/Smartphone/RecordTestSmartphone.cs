using AudioCuesheetEditor.End2EndTests.Models;
using Microsoft.Playwright;

namespace AudioCuesheetEditor.End2EndTests.Tests.Smartphone
{
    [TestClass]
    public class RecordTestSmartphone : PlaywrightTestBase
    {
        protected override string? DeviceName => "iPhone 13";
        [TestMethod]
        public async Task Record_ShouldRecordTracks_WhenTracksAdded()
        {
            var recordView = new RecordView(TestPage);
            await recordView.GotoAsync();
            await recordView.StartRecordingAsync();
            await recordView.AddRecordingTrackAsync("Test Track 1 Artist", "Test Track 1 Title");
            await recordView.AddRecordingTrackAsync("Test Track 2 Artist", "Test Track 2 Title");
            await recordView.StopRecordingAsync();
            await Expect(TestPage.GetByRole(AriaRole.Cell, new() { Name = "Test Track 1 Artist Clear" })).ToBeVisibleAsync();
            await Expect(TestPage.GetByRole(AriaRole.Cell, new() { Name = "Test Track 1 Title Clear" })).ToBeVisibleAsync();
            await Expect(TestPage.GetByRole(AriaRole.Cell, new() { Name = "Test Track 2 Artist Clear" })).ToBeVisibleAsync();
            await Expect(TestPage.GetByRole(AriaRole.Cell, new() { Name = "Test Track 2 Title Clear" })).ToBeVisibleAsync();
        }
    }
}
