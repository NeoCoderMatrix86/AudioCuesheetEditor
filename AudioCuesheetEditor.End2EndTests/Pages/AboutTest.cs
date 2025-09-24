using AudioCuesheetEditor.End2EndTests.Tests;
using Microsoft.Playwright;

namespace AudioCuesheetEditor.End2EndTests.Pages
{
    [TestClass]
    public class AboutTest : PlaywrightTestBase
    {
        //TODO: Move to Tests
        [TestMethod]
        public async Task HasTitleAsync()
        {
            await TestPage.GotoAsync("http://localhost:5132/about");
            await Expect(TestPage).ToHaveTitleAsync("AudioCuesheetEditor");
            await Expect(TestPage.GetByRole(AriaRole.Heading, new() { Name = "About AudioCuesheetEditor" })).ToBeVisibleAsync();
        }
    }
}