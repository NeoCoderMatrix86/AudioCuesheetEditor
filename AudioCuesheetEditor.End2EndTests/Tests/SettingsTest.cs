using AudioCuesheetEditor.End2EndTests.Models;
using Microsoft.Playwright;

namespace AudioCuesheetEditor.End2EndTests.Tests
{
    [TestClass]
    public class SettingsTest : PlaywrightTestBase
    {
        [TestMethod]
        public async Task OpenSettings_ShouldDisplaySettings_WhenSelectingSettings()
        {
            var bar = new AppBar(TestPage);
            await bar.GotoAsync();
            await bar.OpenSettingsAsync();
            await Expect(TestPage.GetByRole(AriaRole.Heading, new() { Name = "Settings" })).ToBeVisibleAsync();
        }
    }
}
