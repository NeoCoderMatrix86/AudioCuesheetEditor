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
    }
}
