using Microsoft.Playwright;
using Microsoft.Playwright.MSTest;

namespace AudioCuesheetEditor.End2EndTests.Pages
{
    [TestClass]
    public class AboutTest : PageTest
    {
        [TestMethod]
        public async Task HasTitle()
        {
            await Page.GotoAsync("http://localhost:5132/about");
            await Expect(Page).ToHaveTitleAsync("AudioCuesheetEditor");
            await Expect(Page.GetByRole(AriaRole.Heading, new() { Name = "About AudioCuesheetEditor" })).ToBeVisibleAsync();
        }
    }
}