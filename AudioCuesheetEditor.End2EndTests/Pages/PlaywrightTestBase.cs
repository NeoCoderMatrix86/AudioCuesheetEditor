using Microsoft.Playwright;
using Microsoft.Playwright.MSTest;

namespace AudioCuesheetEditor.End2EndTests.Pages
{
    public abstract class PlaywrightTestBase : PageTest
    {
        protected IBrowserContext TestContextInstance = null!;
        protected IPage TestPage = null!;

        protected virtual string? DeviceName => null;

        [TestInitialize]
        public async Task TestInitializeAsync()
        {
            //On mobile we start a new context, on desktop we use the default one
            if (DeviceName != null)
            {
                var device = Playwright.Devices[DeviceName];
                TestContextInstance = await Browser.NewContextAsync(device);
                TestPage = await TestContextInstance.NewPageAsync();
            }
            else
            {
                TestContextInstance = Context;
                TestPage = Page;
            }
            await TestContextInstance.Tracing.StartAsync(new()
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
            var failed = new[]
            {
                UnitTestOutcome.Failed,
                UnitTestOutcome.Error,
                UnitTestOutcome.Timeout,
                UnitTestOutcome.Aborted
            }.Contains(TestContext.CurrentTestOutcome);

            await TestContextInstance.Tracing.StopAsync(new()
            {
                Path = failed ? Path.Combine(
                    Environment.CurrentDirectory,
                    "playwright-traces",
                    $"{TestContext.FullyQualifiedTestClassName}.{TestContext.TestName}.zip"
                ) : null,
            });
        }
    }
}
