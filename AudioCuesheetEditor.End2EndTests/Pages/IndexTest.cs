using Microsoft.Playwright;
using Microsoft.Playwright.MSTest;
using System.Text.RegularExpressions;

namespace AudioCuesheetEditor.End2EndTests.Pages
{
    [TestClass]
    public class IndexTest : PageTest
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
            await Page.GotoAsync("http://localhost:5132/");
            await Expect(Page).ToHaveTitleAsync("AudioCuesheetEditor");
            await Expect(Page.GetByRole(AriaRole.Button, new() { Name = "AudioCuesheetEditor" })).ToBeVisibleAsync();
        }

        [TestMethod]
        public async Task CheckSettingsAsync()
        {
            await Page.GotoAsync("http://localhost:5132/");
            await Page.GetByRole(AriaRole.Toolbar).GetByRole(AriaRole.Button).Filter(new() { HasTextRegex = new Regex("^$") }).Nth(3).ClickAsync();
            await Page.Locator("div").Filter(new() { HasTextRegex = new Regex("^Settings$") }).ClickAsync();
            await Expect(Page.GetByRole(AriaRole.Heading, new() { Name = "Settings" })).ToBeVisibleAsync();
        }

        [TestMethod]
        public async Task RecordAsync()
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
            await Page.GetByRole(AriaRole.Button, new() { Name = "Stop recording" }).ClickAsync();
            await Expect(Page.GetByRole(AriaRole.Cell, new() { Name = "Test Track 1 Artist Clear" })).ToBeVisibleAsync();
            await Expect(Page.GetByRole(AriaRole.Cell, new() { Name = "Test Track 1 Title Clear" })).ToBeVisibleAsync();
            await Expect(Page.GetByRole(AriaRole.Cell, new() { Name = "Test Track 2 Artist Clear" })).ToBeVisibleAsync();
            await Expect(Page.GetByRole(AriaRole.Cell, new() { Name = "Test Track 2 Title Clear" })).ToBeVisibleAsync();
        }

        [TestMethod]
        public async Task ImportAsync()
        {
            await Page.GotoAsync("http://localhost:5132/");
            await Page.GetByText("Import view").ClickAsync();
            await Page.GetByRole(AriaRole.Button, new() { Name = "Choose File" }).SetInputFilesAsync(new[] { "Sample_Inputfile.txt" });
            await Page.GetByRole(AriaRole.Button, new() { Name = "Complete" }).ClickAsync();
            await Expect(Page.GetByRole(AriaRole.Cell, new() { Name = "Sample Artist 1 Clear" })).ToBeVisibleAsync();
            await Expect(Page.GetByRole(AriaRole.Cell, new() { Name = ":20:13" }).Nth(1)).ToBeVisibleAsync();
            await Expect(Page.GetByRole(AriaRole.Textbox, new() { Name = "Cuesheet artist" })).ToHaveValueAsync("CuesheetArtist");
            await Page.GetByRole(AriaRole.Textbox, new() { Name = "Cuesheet title" }).ClickAsync();
            await Expect(Page.GetByRole(AriaRole.Group).Filter(new() { HasText = "AudiofileAudiofile Search" }).Locator("input[type=\"file\"]")).ToBeEmptyAsync();
            await Page.GetByText("Import view").ClickAsync();
            await Expect(Page.GetByText("PreviousNext")).ToBeVisibleAsync();
        }

        [TestMethod]
        public async Task ChangeLanguageAsync()
        {
            await Page.GotoAsync("http://localhost:5132/");
            await Page.GetByRole(AriaRole.Button, new() { Name = "Change language" }).ClickAsync();
            await Page.GetByText("German (Germany)").ClickAsync();
            await Expect(Page.GetByRole(AriaRole.Heading, new() { Name = "Abschnitte" })).ToBeVisibleAsync();
            await Expect(Page.GetByRole(AriaRole.Heading, new() { Name = "Allgemeine Informationen" })).ToBeVisibleAsync();
            await Expect(Page.GetByText("Aufnahmeansicht")).ToBeVisibleAsync();
            await Expect(Page.GetByRole(AriaRole.Heading, new() { Name = "Titel" })).ToBeVisibleAsync();
            await Expect(Page.GetByRole(AriaRole.Heading, new() { Name = "Wiedergabe" })).ToBeVisibleAsync();
        }

        [TestMethod]
        public async Task OpenSampleCuesheetAsync()
        {
            await Page.GotoAsync("http://localhost:5132/");
            await Page.GetByRole(AriaRole.Button, new() { Name = "File", Exact = true }).ClickAsync();
            await Page.Locator("div").Filter(new() { HasTextRegex = new Regex("^Open$") }).ClickAsync();
            await Page.Locator("#dropFileInputId_SelectFileDialog").GetByRole(AriaRole.Button, new() { Name = "Choose File" }).ClickAsync();
            await Page.Locator("#dropFileInputId_SelectFileDialog").GetByRole(AriaRole.Button, new() { Name = "Choose File" }).SetInputFilesAsync(new[] { "Sample_Cuesheet.cue" });
            await Expect(Page.GetByRole(AriaRole.Textbox, new() { Name = "Cuesheet artist" })).ToHaveValueAsync("Sample CD Artist");
            await Expect(Page.GetByRole(AriaRole.Textbox, new() { Name = "Cuesheet title" })).ToHaveValueAsync("Sample CD Title");
            await Expect(Page.GetByRole(AriaRole.Cell, new() { Name = "Sample Artist 3 Clear" })).ToBeVisibleAsync();
            await Expect(Page.GetByRole(AriaRole.Cell, new() { Name = ":45:54" }).Nth(1)).ToBeVisibleAsync();
        }

        [TestMethod]
        public async Task OpenProjectFileAsync()
        {
            await Page.GotoAsync("http://localhost:5132/");
            await Page.GetByRole(AriaRole.Button, new() { Name = "File", Exact = true }).ClickAsync();
            await Page.Locator("div").Filter(new() { HasTextRegex = new Regex("^Open$") }).ClickAsync();
            await Page.Locator("#dropFileInputId_SelectFileDialog").GetByRole(AriaRole.Button, new() { Name = "Choose File" }).ClickAsync();
            await Page.Locator("#dropFileInputId_SelectFileDialog").GetByRole(AriaRole.Button, new() { Name = "Choose File" }).SetInputFilesAsync(new[] { "Sample_Project.ace" });
            await Expect(Page.GetByRole(AriaRole.Textbox, new() { Name = "Cuesheet artist" })).ToHaveValueAsync("Sample CD Artist");
            await Expect(Page.GetByRole(AriaRole.Textbox, new() { Name = "Cuesheet title" })).ToHaveValueAsync("Sample CD Title");
            await Expect(Page.GetByRole(AriaRole.Cell, new() { Name = "Sample Title 2 Clear" })).ToBeVisibleAsync();
            await Expect(Page.GetByRole(AriaRole.Cell, new() { Name = ":09:23" }).Nth(1)).ToBeVisibleAsync();
            await Expect(Page.GetByRole(AriaRole.Cell, new() { Name = ":45:54" }).First).ToBeVisibleAsync();
        }

        [TestMethod]
        public async Task RenameAudiofileTestAsync()
        {
            await Page.GotoAsync("http://localhost:5132/");
            await Page.GetByRole(AriaRole.Group).Filter(new() { HasText = "AudiofileAudiofile Search" }).GetByRole(AriaRole.Button).Nth(3).ClickAsync();
            await Expect(Page.GetByText("Download file Rename file")).ToMatchAriaSnapshotAsync("- paragraph: Download file\n- paragraph: Rename file");
            await Page.Locator("[id^='overlay']").ClickAsync();
            await Page.GetByRole(AriaRole.Group).Filter(new() { HasText = "AudiofileAudiofile Search" }).Locator("input[type=\"file\"]").ClickAsync();
            await Page.GetByRole(AriaRole.Group).Filter(new() { HasText = "AudiofileAudiofile Search" }).Locator("input[type=\"file\"]").SetInputFilesAsync(new[] { "Kalimba.mp3" });
            await Page.GetByRole(AriaRole.Group).Filter(new() { HasText = "AudiofileAudiofile Search" }).GetByRole(AriaRole.Button).Nth(3).ClickAsync();
            await Expect(Page.GetByText("Download file Rename file")).ToMatchAriaSnapshotAsync("- paragraph: Download file\n- paragraph: Rename file");
            await Page.GetByText("Rename file").ClickAsync();
            await Page.GetByRole(AriaRole.Textbox, new() { Name = "New file name" }).FillAsync("Kalimba test 123.mp3");
            await Page.GetByRole(AriaRole.Button, new() { Name = "Ok" }).ClickAsync();
            await Expect(Page.Locator("#app")).ToMatchAriaSnapshotAsync("- textbox \"Cuesheet artist\"\n- group \"Cuesheet artist\"\n- text: Cuesheet artist\n- textbox \"Cuesheet title\"\n- group \"Cuesheet title\"\n- text: Cuesheet title\n- group:\n  - button \"Choose File\"\n  - textbox \"Audiofile\": /Kalimba test \\d+\\.mp3/\n  - group \"Audiofile\"\n  - text: Audiofile\n  - button \"Search\"\n  - button\n  - button\n- group:\n  - button \"Choose File\"\n  - textbox \"CD Textfile\": No file selected\n  - group \"CD Textfile\"\n  - text: CD Textfile\n  - button \"Search\"\n  - button [disabled]\n  - button\n- textbox \"Cataloguenumber\"\n- group \"Cataloguenumber\"\n- text: Cataloguenumber");
        }
    }
}
