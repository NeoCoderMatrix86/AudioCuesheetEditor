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
            var indexPage = new IndexPage(TestPage);
            await indexPage.GotoAsync();
            await indexPage.OpenSettingsAsync();
            await Expect(TestPage.GetByRole(AriaRole.Heading, new() { Name = "Settings" })).ToBeVisibleAsync();
        }
    }
}
