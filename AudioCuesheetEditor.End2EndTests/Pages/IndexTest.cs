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
            await Context.Tracing.StopAsync(new()
            {
                Path = Path.Combine(
                    Environment.CurrentDirectory,
                    "playwright-traces",
                    $"{TestContext.FullyQualifiedTestClassName}.{TestContext.TestName}.zip"
                )
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

        [TestMethod]
        public async Task Record()
        {
            await Page.GotoAsync("http://localhost:5132/");
            await Page.GetByText("Record view").ClickAsync();
            await Page.GetByRole(AriaRole.Button, new() { Name = "Start recording" }).ClickAsync();
            await Page.GetByRole(AriaRole.Textbox, new() { Name = "Artist", Exact = true }).ClickAsync();
            await Page.GetByRole(AriaRole.Textbox, new() { Name = "Artist", Exact = true }).FillAsync("Test Track 1 Artist");
            await Page.GetByRole(AriaRole.Textbox, new() { Name = "Artist", Exact = true }).PressAsync("Tab");
            await Page.GetByRole(AriaRole.Textbox, new() { Name = "Title", Exact = true }).FillAsync("Test Track 1 Title");
            await Page.GetByRole(AriaRole.Textbox, new() { Name = "Title", Exact = true }).PressAsync("Tab");
            await Page.GetByRole(AriaRole.Button, new() { Name = "Add track" }).ClickAsync();
            await Page.GetByRole(AriaRole.Textbox, new() { Name = "Artist", Exact = true }).FillAsync("Test Track 2 Artist");
            await Page.GetByRole(AriaRole.Textbox, new() { Name = "Artist", Exact = true }).PressAsync("Tab");
            await Page.GetByRole(AriaRole.Textbox, new() { Name = "Title", Exact = true }).FillAsync("Test Track 2 Title");
            await Page.GetByRole(AriaRole.Textbox, new() { Name = "Title", Exact = true }).PressAsync("Tab");
            await Page.GetByRole(AriaRole.Button, new() { Name = "Add track" }).ClickAsync();
            await Page.Locator(".mud-overlay").ClickAsync();
            await Page.GetByRole(AriaRole.Button, new() { Name = "Stop recording" }).ClickAsync();
            await Expect(Page.GetByRole(AriaRole.Cell, new() { Name = "Test Track 1 Artist Clear" })).ToBeVisibleAsync();
            await Expect(Page.GetByRole(AriaRole.Cell, new() { Name = "Test Track 1 Title Clear" })).ToBeVisibleAsync();
            await Expect(Page.GetByRole(AriaRole.Cell, new() { Name = "Test Track 2 Artist Clear" })).ToBeVisibleAsync();
            await Expect(Page.GetByRole(AriaRole.Cell, new() { Name = "Test Track 2 Title Clear" })).ToBeVisibleAsync();
        }

        [TestMethod]
        public async Task ChangeLanguage()
        {
            // We need to use a new context because the default one doesn't work with changing a language
            var context = await Browser.NewContextAsync();
            var page = await context.NewPageAsync();
            await page.GotoAsync("http://localhost:5132/");
            await page.GetByRole(AriaRole.Button, new() { Name = "Change language" }).ClickAsync();
            await page.Locator("div").Filter(new() { HasTextRegex = new Regex("^German \\(Germany\\)$") }).ClickAsync();
            await Expect(page.Locator("#app")).ToContainTextAsync("Allgemeine Informationen");
        }

        [TestMethod]
        public async Task Import()
        {
            await Page.GotoAsync("http://localhost:5132/");
            await Page.GetByText("Import view").ClickAsync();
            await Page.GetByRole(AriaRole.Button, new() { Name = "Choose File" }).ClickAsync();
            await Page.GetByRole(AriaRole.Button, new() { Name = "Choose File" }).SetInputFilesAsync(new[] { "../../../../AudioCuesheetEditor/wwwroot/samples/Sample_Inputfile.txt" });
            await Page.GetByRole(AriaRole.Button, new() { Name = "Complete" }).ClickAsync();
            await Expect(Page.GetByRole(AriaRole.Cell, new() { Name = "Sample Artist 1 Clear" })).ToBeVisibleAsync();
            await Expect(Page.GetByRole(AriaRole.Cell, new() { Name = ":20:13" }).Nth(1)).ToBeVisibleAsync();
            await Expect(Page.GetByRole(AriaRole.Textbox, new() { Name = "Cuesheet artist" })).ToHaveValueAsync("CuesheetArtist");
            await Page.GetByRole(AriaRole.Textbox, new() { Name = "Cuesheet title" }).ClickAsync();
            await Expect(Page.GetByRole(AriaRole.Group).Filter(new() { HasText = "AudiofileAudiofile Search" }).Locator("input[type=\"file\"]")).ToBeEmptyAsync();
        }

        [TestMethod]
        public async Task GenerateCuesheet()
        {
            await Page.GotoAsync("http://localhost:5132/");
            await Page.GetByRole(AriaRole.Button, new() { Name = "File", Exact = true }).ClickAsync();
            await Page.GetByText("Open").ClickAsync();
            await Page.Locator("#dropFileInputId_SelectFileDialog").GetByRole(AriaRole.Button, new() { Name = "Choose File" }).ClickAsync();
            await Page.Locator("#dropFileInputId_SelectFileDialog").GetByRole(AriaRole.Button, new() { Name = "Choose File" }).SetInputFilesAsync(new[] { "../../../../AudioCuesheetEditor/wwwroot/samples/Sample_Cuesheet.cue" });
            await Expect(Page.GetByRole(AriaRole.Textbox, new() { Name = "Cuesheet artist" })).ToHaveValueAsync("Sample CD Artist");
            await Expect(Page.GetByRole(AriaRole.Textbox, new() { Name = "Cuesheet title" })).ToHaveValueAsync("Sample CD Title");
            await Expect(Page.GetByRole(AriaRole.Cell, new() { Name = "Sample Artist 6" }).GetByRole(AriaRole.Textbox)).ToHaveValueAsync("Sample Artist 6");
            await Expect(Page.GetByRole(AriaRole.Row, new() { Name = "Increment Decrement Sample Artist 7 Clear Sample Title 7 Clear 00:31:54 00:45:" }).GetByRole(AriaRole.Textbox).Nth(3)).ToHaveValueAsync("00:45:54");
            await Page.GetByRole(AriaRole.Button, new() { Name = "File", Exact = true }).ClickAsync();
            await Page.GetByText("Export").ClickAsync();
            await Page.GetByText("Cuesheet", new() { Exact = true }).ClickAsync();
            var download = await Page.RunAndWaitForDownloadAsync(async () =>
            {
                await Page.GetByRole(AriaRole.Row, new() { Name = "Cuesheet.cue 00:00:00 00:45:" }).GetByRole(AriaRole.Button).ClickAsync();
            });
            // Read the downloaded file content as text
            var cuesheetContent = await download.PathAsync();
            var actualText = await File.ReadAllTextAsync(cuesheetContent);
            var expectedText = @"TITLE ""Sample CD Title""
PERFORMER ""Sample CD Artist""
FILE ""Sample.mp3"" MP3
	TRACK 01 AUDIO
		TITLE ""Sample Title 1""
		PERFORMER ""Sample Artist 1""
		INDEX 01 00:00:00
	TRACK 02 AUDIO
		TITLE ""Sample Title 2""
		PERFORMER ""Sample Artist 2""
		INDEX 01 05:00:00
	TRACK 03 AUDIO
		TITLE ""Sample Title 3""
		PERFORMER ""Sample Artist 3""
		INDEX 01 09:23:00
	TRACK 04 AUDIO
		TITLE ""Sample Title 4""
		PERFORMER ""Sample Artist 4""
		INDEX 01 15:54:00
	TRACK 05 AUDIO
		TITLE ""Sample Title 5""
		PERFORMER ""Sample Artist 5""
		INDEX 01 20:13:00
	TRACK 06 AUDIO
		TITLE ""Sample Title 6""
		PERFORMER ""Sample Artist 6""
		INDEX 01 24:54:00
	TRACK 07 AUDIO
		TITLE ""Sample Title 7""
		PERFORMER ""Sample Artist 7""
		INDEX 01 31:54:00
	TRACK 08 AUDIO
		TITLE ""Sample Title 8""
		PERFORMER ""Sample Artist 8""
		INDEX 01 45:54:00
";
            Assert.AreEqual(expectedText.Replace("\r\n", "\n"), actualText.Replace("\r\n", "\n"));
        }
    }
}
