using AudioCuesheetEditor.End2EndTests.Tests;
using Microsoft.Playwright;
using System.Text.RegularExpressions;

namespace AudioCuesheetEditor.End2EndTests.Pages
{
    [TestClass]
    public class IndexTest : PlaywrightTestBase
    {
        //TODO: Implement Page object models and use inside tests, before adding a mobile test
        [TestMethod]
        public async Task HasTitleAsync()
        {
            await TestPage.GotoAsync("http://localhost:5132/");
            await Expect(TestPage).ToHaveTitleAsync("AudioCuesheetEditor");
            await Expect(TestPage.GetByRole(AriaRole.Button, new() { Name = "AudioCuesheetEditor" })).ToBeVisibleAsync();
        }

        [TestMethod]
        public async Task OpenSampleCuesheetAsync()
        {
            await TestPage.GotoAsync("http://localhost:5132/");
            await TestPage.GetByRole(AriaRole.Button, new() { Name = "File", Exact = true }).ClickAsync();
            await TestPage.Locator("div").Filter(new() { HasTextRegex = new Regex("^Open$") }).ClickAsync();
            await TestPage.Locator("#dropFileInputId_SelectFileDialog").GetByRole(AriaRole.Button, new() { Name = "Choose File" }).ClickAsync();
            await TestPage.Locator("#dropFileInputId_SelectFileDialog").GetByRole(AriaRole.Button, new() { Name = "Choose File" }).SetInputFilesAsync(new[] { "Sample_Cuesheet.cue" });
            await Expect(TestPage.GetByRole(AriaRole.Textbox, new() { Name = "Cuesheet artist" })).ToHaveValueAsync("Sample CD Artist");
            await Expect(TestPage.GetByRole(AriaRole.Textbox, new() { Name = "Cuesheet title" })).ToHaveValueAsync("Sample CD Title");
            await Expect(TestPage.GetByRole(AriaRole.Cell, new() { Name = "Sample Artist 3 Clear" })).ToBeVisibleAsync();
            await Expect(TestPage.GetByRole(AriaRole.Cell, new() { Name = ":45:54" }).Nth(1)).ToBeVisibleAsync();
            await TestPage.GetByRole(AriaRole.Toolbar).GetByRole(AriaRole.Button).Filter(new() { HasTextRegex = new Regex("^$") }).First.ClickAsync();
            await Expect(TestPage.GetByRole(AriaRole.Paragraph).Filter(new() { HasText = "Tracks has invalid Count (0)!" })).ToBeVisibleAsync();
        }

        [TestMethod]
        public async Task OpenProjectFileAsync()
        {
            await TestPage.GotoAsync("http://localhost:5132/");
            await TestPage.GetByRole(AriaRole.Button, new() { Name = "File", Exact = true }).ClickAsync();
            await TestPage.Locator("div").Filter(new() { HasTextRegex = new Regex("^Open$") }).ClickAsync();
            await TestPage.Locator("#dropFileInputId_SelectFileDialog").GetByRole(AriaRole.Button, new() { Name = "Choose File" }).ClickAsync();
            await TestPage.Locator("#dropFileInputId_SelectFileDialog").GetByRole(AriaRole.Button, new() { Name = "Choose File" }).SetInputFilesAsync(new[] { "Sample_Project.ace" });
            await Expect(TestPage.GetByRole(AriaRole.Textbox, new() { Name = "Cuesheet artist" })).ToHaveValueAsync("Sample CD Artist");
            await Expect(TestPage.GetByRole(AriaRole.Textbox, new() { Name = "Cuesheet title" })).ToHaveValueAsync("Sample CD Title");
            await Expect(TestPage.GetByRole(AriaRole.Cell, new() { Name = "Sample Title 2 Clear" })).ToBeVisibleAsync();
            await Expect(TestPage.GetByRole(AriaRole.Cell, new() { Name = ":09:23" }).Nth(1)).ToBeVisibleAsync();
            await Expect(TestPage.GetByRole(AriaRole.Cell, new() { Name = ":45:54" }).First).ToBeVisibleAsync();
            await TestPage.GetByRole(AriaRole.Toolbar).GetByRole(AriaRole.Button).Filter(new() { HasTextRegex = new Regex("^$") }).First.ClickAsync();
            await Expect(TestPage.GetByRole(AriaRole.Paragraph).Filter(new() { HasText = "Tracks has invalid Count (0)!" })).ToBeVisibleAsync();
        }

        [TestMethod]
        public async Task RenameAudiofileTestAsync()
        {
            await TestPage.GotoAsync("http://localhost:5132/");
            await TestPage.GetByRole(AriaRole.Group).Filter(new() { HasText = "AudiofileAudiofile Search" }).GetByRole(AriaRole.Button).Nth(3).ClickAsync();
            await Expect(TestPage.GetByText("Download file Rename file")).ToMatchAriaSnapshotAsync("- paragraph: Download file\n- paragraph: Rename file");
            await TestPage.Locator("[id^='overlay']").ClickAsync();
            await TestPage.GetByRole(AriaRole.Group).Filter(new() { HasText = "AudiofileAudiofile Search" }).Locator("input[type=\"file\"]").ClickAsync();
            await TestPage.GetByRole(AriaRole.Group).Filter(new() { HasText = "AudiofileAudiofile Search" }).Locator("input[type=\"file\"]").SetInputFilesAsync(new[] { "Kalimba.mp3" });
            await TestPage.GetByRole(AriaRole.Group).Filter(new() { HasText = "AudiofileAudiofile Search" }).GetByRole(AriaRole.Button).Nth(3).ClickAsync();
            await Expect(TestPage.GetByText("Download file Rename file")).ToMatchAriaSnapshotAsync("- paragraph: Download file\n- paragraph: Rename file");
            await TestPage.GetByText("Rename file").ClickAsync();
            await TestPage.GetByRole(AriaRole.Textbox, new() { Name = "New file name" }).FillAsync("Kalimba test 123.mp3");
            await TestPage.GetByRole(AriaRole.Button, new() { Name = "Ok" }).ClickAsync();
            await Expect(TestPage.Locator("#app")).ToMatchAriaSnapshotAsync("- textbox \"Cuesheet artist\"\n- group \"Cuesheet artist\"\n- text: Cuesheet artist\n- textbox \"Cuesheet title\"\n- group \"Cuesheet title\"\n- text: Cuesheet title\n- group:\n  - button \"Choose File\"\n  - textbox \"Audiofile\": /Kalimba test \\d+\\.mp3/\n  - group \"Audiofile\"\n  - text: Audiofile\n  - button \"Search\"\n  - button\n  - button\n- group:\n  - button \"Choose File\"\n  - textbox \"CD Textfile\": No file selected\n  - group \"CD Textfile\"\n  - text: CD Textfile\n  - button \"Search\"\n  - button [disabled]\n  - button\n- textbox \"Cataloguenumber\"\n- group \"Cataloguenumber\"\n- text: Cataloguenumber");
        }

        [TestMethod]
        public async Task ImportUndoRedoTestAsync()
        {
            await TestPage.GotoAsync("http://localhost:5132/");
            await TestPage.GetByText("Import view").ClickAsync();
            await TestPage.GetByRole(AriaRole.Button, new() { Name = "Choose File" }).ClickAsync();
            await TestPage.GetByRole(AriaRole.Button, new() { Name = "Choose File" }).SetInputFilesAsync("Textimport with Cuesheetdata.txt");
            await TestPage.GetByRole(AriaRole.Textbox, new() { Name = "Scheme common data" }).FillAsync("Artist - Title - ");
            await TestPage.Locator("div").Filter(new() { HasTextRegex = new Regex("^Scheme common data$") }).GetByRole(AriaRole.Button).Nth(1).ClickAsync();
            await TestPage.GetByRole(AriaRole.Paragraph).Filter(new() { HasText = "Cataloguenumber" }).ClickAsync();
            await Expect(TestPage.GetByRole(AriaRole.Toolbar)).ToMatchAriaSnapshotAsync("- button \"undo\" [disabled]");
            await Expect(TestPage.GetByRole(AriaRole.Toolbar)).ToMatchAriaSnapshotAsync("- button \"redo\" [disabled]");
            await TestPage.GetByRole(AriaRole.Button, new() { Name = "Complete" }).ClickAsync();
            await Expect(TestPage.GetByRole(AriaRole.Button, new() { Name = "undo" })).ToMatchAriaSnapshotAsync("- button \"undo\"");
            await Expect(TestPage.GetByRole(AriaRole.Toolbar)).ToMatchAriaSnapshotAsync("- button \"redo\" [disabled]");
            await TestPage.GetByRole(AriaRole.Button, new() { Name = "undo" }).ClickAsync();
            await Expect(TestPage.GetByRole(AriaRole.Toolbar)).ToMatchAriaSnapshotAsync("- button \"undo\" [disabled]");
            await Expect(TestPage.GetByRole(AriaRole.Button, new() { Name = "redo" })).ToMatchAriaSnapshotAsync("- button \"redo\"");
            await TestPage.GetByRole(AriaRole.Button, new() { Name = "redo" }).ClickAsync();
            await Expect(TestPage.GetByRole(AriaRole.Textbox, new() { Name = "Cuesheet artist" })).ToHaveValueAsync("DJFreezeT");
            await Expect(TestPage.GetByRole(AriaRole.Textbox, new() { Name = "Cuesheet title" })).ToHaveValueAsync("Rabbit Hole Mix");
            await Expect(TestPage.GetByRole(AriaRole.Textbox, new() { Name = "Cataloguenumber" })).ToHaveValueAsync("01 23456 78912 3");
            await Expect(TestPage.GetByRole(AriaRole.Cell, new() { Name = "Einmusik Clear" })).ToBeVisibleAsync();
            await Expect(TestPage.GetByRole(AriaRole.Cell, new() { Name = "Missing Path (Original Mix)" })).ToBeVisibleAsync();
        }

        [TestMethod]
        public async Task ImportTestTraktorAsync()
        {
            await TestPage.GotoAsync("http://localhost:5132/");
            await TestPage.GetByText("Import view").ClickAsync();
            await TestPage.GetByRole(AriaRole.Button, new() { Name = "Choose File" }).SetInputFilesAsync(new[] { "Traktor Export.html" });
            await TestPage.GetByText("Textfile (common data in").ClickAsync();
            await TestPage.GetByText("Traktor history").ClickAsync();
            await Expect(TestPage.GetByRole(AriaRole.Cell, new() { Name = "Arba Han Clear" })).ToBeVisibleAsync();
            await Expect(TestPage.GetByRole(AriaRole.Cell, new() { Name = ":48:53" }).Nth(1)).ToBeVisibleAsync();
            await Expect(TestPage.GetByRole(AriaRole.Cell, new() { Name = "Acid Rain Clear" })).ToBeVisibleAsync();
            await Expect(TestPage.GetByRole(AriaRole.Cell, new() { Name = ":48:40" }).Nth(1)).ToBeVisibleAsync();
            await TestPage.GetByRole(AriaRole.Button, new() { Name = "Complete" }).ClickAsync();
            await Expect(TestPage.GetByRole(AriaRole.Cell, new() { Name = "Space Yoda (Snyl Remix) Clear" })).ToBeVisibleAsync();
        }
    }
}
