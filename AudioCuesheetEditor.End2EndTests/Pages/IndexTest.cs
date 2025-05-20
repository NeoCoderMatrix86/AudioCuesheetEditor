using Microsoft.Playwright;
using Microsoft.Playwright.MSTest;
using System.Text.RegularExpressions;

namespace AudioCuesheetEditor.End2EndTests.Pages
{
    [TestClass]
    public class IndexTest : PageTest
    {
        [TestInitialize]
        public async Task TestInitialize()
        {
            await Context.Tracing.StartAsync(new()
            {
                Title = $"{TestContext.FullyQualifiedTestClassName}.{TestContext.TestName}",
                Screenshots = true,
                Snapshots = true,
                Sources = true
            });
        }

        [TestCleanup]
        public async Task TestCleanup()
        {
            var failed = new[] { UnitTestOutcome.Failed, UnitTestOutcome.Error, UnitTestOutcome.Timeout, UnitTestOutcome.Aborted }.Contains(TestContext.CurrentTestOutcome);

            await Context.Tracing.StopAsync(new()
            {
                Path = failed ? Path.Combine(
                    Environment.CurrentDirectory,
                    "playwright-traces",
                    $"{TestContext.FullyQualifiedTestClassName}.{TestContext.TestName}.zip"
                ) : null,
            });
        }

        [TestMethod]
        public async Task HasTitle()
        {
            await Page.GotoAsync("http://localhost:5132/");
            await Expect(Page).ToHaveTitleAsync("AudioCuesheetEditor");
            await Expect(Page.GetByRole(AriaRole.Button, new() { Name = "AudioCuesheetEditor" })).ToBeVisibleAsync();
        }

        [TestMethod]
        public async Task CheckSettings()
        {
            await Page.GotoAsync("http://localhost:5132/");
            await Page.GetByRole(AriaRole.Toolbar).GetByRole(AriaRole.Button).Filter(new() { HasTextRegex = new Regex("^$") }).Nth(3).ClickAsync();
            await Page.Locator("div").Filter(new() { HasTextRegex = new Regex("^Settings$") }).ClickAsync();
            await Expect(Page.GetByRole(AriaRole.Heading, new() { Name = "Settings" })).ToBeVisibleAsync();
        }
        //TODO: Change language test
        //TODO: Switch view test
        //TODO: Import sample cuesheet test
    }
}
