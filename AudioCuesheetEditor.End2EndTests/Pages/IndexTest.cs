using Microsoft.Playwright;
using Microsoft.Playwright.MSTest;

namespace AudioCuesheetEditor.End2EndTests.Pages
{
    [TestClass]
    public class IndexTest : PageTest
    {
        [TestMethod]
        public async Task HasTitle()
        {
            await Page.GotoAsync("http://localhost:5132/");
            await Expect(Page).ToHaveTitleAsync("AudioCuesheetEditor");
            await Expect(Page.GetByRole(AriaRole.Button, new() { Name = "AudioCuesheetEditor" })).ToBeVisibleAsync();
        }

        //TODO: Change language test
        //TODO: Switch view test
        //TODO: Settings test
        //TODO: Import sample cuesheet test
    }
}
