using AudioCuesheetEditor.End2EndTests.Models;

namespace AudioCuesheetEditor.End2EndTests.Tests
{
    [TestClass]
    public class BasicTest : PlaywrightTestBase
    {
        [TestMethod]
        public async Task Application_HasTitle_WhenBrowsingIndex()
        {
            var detailView = new DetailView(TestPage);
            await detailView.GotoAsync();
            var appBar = new AppBar(TestPage);
            await Expect(appBar.HomeButton).ToBeVisibleAsync();
            await Expect(TestPage).ToHaveTitleAsync("AudioCuesheetEditor");
        }

        [TestMethod]
        public async Task AboutHeader_HasTitle_WhenBrowsingAbout()
        {
            var about = new About(TestPage);
            await about.GotoAsync();
            await Expect(TestPage).ToHaveTitleAsync("AudioCuesheetEditor");
            await Expect(about.AboutHeading).ToBeVisibleAsync();
        }
    }
}
