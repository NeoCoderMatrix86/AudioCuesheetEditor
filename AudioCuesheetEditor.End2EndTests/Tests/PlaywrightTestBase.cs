//This file is part of AudioCuesheetEditor.

//AudioCuesheetEditor is free software: you can redistribute it and/or modify
//it under the terms of the GNU General Public License as published by
//the Free Software Foundation, either version 3 of the License, or
//(at your option) any later version.

//AudioCuesheetEditor is distributed in the hope that it will be useful,
//but WITHOUT ANY WARRANTY; without even the implied warranty of
//MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
//GNU General Public License for more details.

//You should have received a copy of the GNU General Public License
//along with Foobar.  If not, see
//<http: //www.gnu.org/licenses />.
using Microsoft.Playwright;
using Microsoft.Playwright.MSTest;

namespace AudioCuesheetEditor.End2EndTests.Tests
{
    public abstract class PlaywrightTestBase : PageTest
    {
        private IBrowserContext _testContextInstance = null!;
        protected IPage TestPage = null!;

        protected virtual string? DeviceName => null;

        [TestInitialize]
        public async Task TestInitializeAsync()
        {
            //On mobile we start a new context, on desktop we use the default one
            if (DeviceName != null)
            {
                var device = Playwright.Devices[DeviceName];
                _testContextInstance = await Browser.NewContextAsync(device);
                TestPage = await _testContextInstance.NewPageAsync();
            }
            else
            {
                _testContextInstance = Context;
                TestPage = Page;
            }
            await _testContextInstance.Tracing.StartAsync(new()
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

            await _testContextInstance.Tracing.StopAsync(new()
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
