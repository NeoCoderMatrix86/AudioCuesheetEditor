using Microsoft.Playwright;
using Microsoft.Playwright.MSTest;

namespace AudioCuesheetEditor.End2EndTests.Pages
{
    [TestClass]
    public class AboutTest : PageTest
    {
        [TestInitialize]
        public async Task TestInitializeAsync()
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
        public async Task TestCleanupAsync()
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
        public async Task HasTitleAsync()
        {
            await Page.GotoAsync("http://localhost:5132/about");
            await Expect(Page).ToHaveTitleAsync("AudioCuesheetEditor");
            await Expect(Page.GetByRole(AriaRole.Heading, new() { Name = "About AudioCuesheetEditor" })).ToBeVisibleAsync();
        }
    }
}