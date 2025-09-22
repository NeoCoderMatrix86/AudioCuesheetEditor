using AudioCuesheetEditor.End2EndTests.Pages;
using Microsoft.Playwright;

namespace AudioCuesheetEditor.End2EndTests
{
    [TestClass]
    public class SettingsTest : PlaywrightTestBase
    {
        [TestMethod]
        public async Task OpenSettingsAsync()
        {
            var page = new AppBar(TestPage);
            await page.GotoAsync();
            await page.OpenSettingsAsync();
            await Expect(TestPage.GetByRole(AriaRole.Heading, new() { Name = "Settings" })).ToBeVisibleAsync();
        }
    }
}
