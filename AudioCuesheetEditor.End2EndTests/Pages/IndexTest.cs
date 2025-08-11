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
            
            // Added End2End Test
            // Test localised export placeholders
            await Page.GetByText("Exportansicht").ClickAsync();
            await Page.GetByRole(AriaRole.Button, new() {Name = "Exportprofil hinzufügen"}).ClickAsync();
            
             // Open placeholder dropdown for tracks 
            await Page.GetByRole(AriaRole.Button, new() { Name = "Platzhalter hinzufügen"}).First.ClickAsync();

            // Confirm that the German placeholders are visible
            await Expect(Page.GetByRole(AriaRole.Menuitem, new() { Name = "Künstler"})).ToBeVisibleAsync();
            await Expect(Page.GetByRole(AriaRole.Menuitem, new() {Name = "Titel"})).ToBeVisibleAsync();
            await Expect(Page.GetByRole(AriaRole.Menuitem, new() { Name = "Album"})).ToBeVisibleAsync();
            
            // Close dropdown and open cuesheet placeholder dropdown
            await Page.Keyboard.PressAsync("Escape");
            await Page.GetByRole(AriaRole.Button, new() {Name = "Platzhalter hinzufügen"}).Last.ClickAsync();

            // Confirm that the German cuesheet placeholders are visible
            await Expect(Page.GetByRole(AriaRole.Menuitem, new() { Name = "Cuesheet Titel"})).ToBeVisibleAsync();
            await Expect(Page.GetByRole(AriaRole.Menuitem, new() { Name = "Interpret" })).ToBeVisibleAsync();
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
            await Page.GetByRole(AriaRole.Toolbar).GetByRole(AriaRole.Button).Filter(new() { HasTextRegex = new Regex("^$") }).First.ClickAsync();
            await Expect(Page.GetByRole(AriaRole.Paragraph).Filter(new() { HasText = "Tracks has invalid Count (0)!" })).ToBeVisibleAsync();
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
            await Page.GetByRole(AriaRole.Toolbar).GetByRole(AriaRole.Button).Filter(new() { HasTextRegex = new Regex("^$") }).First.ClickAsync();
            await Expect(Page.GetByRole(AriaRole.Paragraph).Filter(new() { HasText = "Tracks has invalid Count (0)!" })).ToBeVisibleAsync();
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

        [TestMethod]
        public async Task ImportUndoRedoTestAsync()
        {
            await Page.GotoAsync("http://localhost:5132/");
            await Page.GetByText("Import view").ClickAsync();
            await Page.GetByRole(AriaRole.Button, new() { Name = "Choose File" }).ClickAsync();
            await Page.GetByRole(AriaRole.Button, new() { Name = "Choose File" }).SetInputFilesAsync(new[] { "Textimport with Cuesheetdata.txt" });
            await Page.GetByRole(AriaRole.Textbox, new() { Name = "Scheme common data" }).FillAsync("Artist - Title - ");
            await Page.GetByRole(AriaRole.Group).Filter(new() { HasText = "Scheme common dataScheme" }).GetByRole(AriaRole.Button).Nth(1).ClickAsync();
            await Page.GetByRole(AriaRole.Paragraph).Filter(new() { HasText = "Cataloguenumber" }).ClickAsync();
            await Expect(Page.GetByRole(AriaRole.Toolbar)).ToMatchAriaSnapshotAsync("- button \"undo\" [disabled]");
            await Expect(Page.GetByRole(AriaRole.Toolbar)).ToMatchAriaSnapshotAsync("- button \"redo\" [disabled]");
            await Page.GetByRole(AriaRole.Button, new() { Name = "Complete" }).ClickAsync();
            await Expect(Page.GetByRole(AriaRole.Button, new() { Name = "undo" })).ToMatchAriaSnapshotAsync("- button \"undo\"");
            await Expect(Page.GetByRole(AriaRole.Toolbar)).ToMatchAriaSnapshotAsync("- button \"redo\" [disabled]");
            await Page.GetByRole(AriaRole.Button, new() { Name = "undo" }).ClickAsync();
            await Expect(Page.GetByRole(AriaRole.Toolbar)).ToMatchAriaSnapshotAsync("- button \"undo\" [disabled]");
            await Expect(Page.GetByRole(AriaRole.Button, new() { Name = "redo" })).ToMatchAriaSnapshotAsync("- button \"redo\"");
            await Page.GetByRole(AriaRole.Button, new() { Name = "redo" }).ClickAsync();
            await Expect(Page.GetByRole(AriaRole.Textbox, new() { Name = "Cuesheet artist" })).ToHaveValueAsync("DJFreezeT");
            await Expect(Page.GetByRole(AriaRole.Textbox, new() { Name = "Cuesheet title" })).ToHaveValueAsync("Rabbit Hole Mix");
            await Expect(Page.GetByRole(AriaRole.Textbox, new() { Name = "Cataloguenumber" })).ToHaveValueAsync("0123456789123");
            await Expect(Page.GetByRole(AriaRole.Cell, new() { Name = "Einmusik Clear" })).ToBeVisibleAsync();
            await Expect(Page.GetByRole(AriaRole.Cell, new() { Name = "Missing Path (Original Mix)" })).ToBeVisibleAsync();
        }

        [TestMethod]
        public async Task ImportTestBug54Async()
        {
            await Page.GotoAsync("http://localhost:5132/");
            await Page.GetByText("Import view").ClickAsync();
            await Page.GetByRole(AriaRole.Button, new() { Name = "Choose File" }).ClickAsync();
            await Page.GetByRole(AriaRole.Button, new() { Name = "Choose File" }).SetInputFilesAsync(new[] { "Textimport-Bug-#54.txt" });
            await Page.GetByText("Textfile (common data in").ClickAsync();
            await Page.GetByText("Textfile (just track data)").ClickAsync();
            await Expect(Page.GetByRole(AriaRole.Tabpanel)).ToMatchAriaSnapshotAsync("- table:\n  - rowgroup:\n    - row \"# Sort Show Column Options Artist Sort Show Column Options Title Sort Show Column Options Begin Sort Show Column Options End Sort Show Column Options Length Sort Show Column Options\":\n      - columnheader:\n        - checkbox\n      - columnheader \"# Sort Show Column Options\":\n        - button \"Sort\"\n        - button \"Show Column Options\"\n      - columnheader \"Artist Sort Show Column Options\":\n        - button \"Sort\"\n        - button \"Show Column Options\"\n      - columnheader \"Title Sort Show Column Options\":\n        - button \"Sort\"\n        - button \"Show Column Options\"\n      - columnheader \"Begin Sort Show Column Options\":\n        - button \"Sort\"\n        - button \"Show Column Options\"\n      - columnheader \"End Sort Show Column Options\":\n        - button \"Sort\"\n        - button \"Show Column Options\"\n      - columnheader \"Length Sort Show Column Options\":\n        - button \"Sort\"\n        - button \"Show Column Options\"\n  - rowgroup:\n    - row /Increment Decrement Adriatique Clear X\\. Clear \\d+:\\d+:\\d+ \\d+:\\d+:\\d+ \\d+:\\d+:\\d+/:\n      - cell:\n        - checkbox\n      - cell \"Increment Decrement\":\n        - spinbutton: \"1\"\n        - button \"Increment\"\n        - button \"Decrement\"\n      - cell \"Adriatique Clear\":\n        - textbox: Adriatique\n        - button \"Clear\"\n        - button\n      - cell \"X. Clear\":\n        - textbox: X.\n        - button \"Clear\"\n        - button\n      - cell /\\d+:\\d+:\\d+/:\n        - textbox: /\\d+:\\d+:\\d+/\n      - cell /\\d+:\\d+:\\d+/:\n        - textbox: /\\d+:\\d+:\\d+/\n      - cell /\\d+:\\d+:\\d+/:\n        - textbox: /\\d+:\\d+:\\d+/\n    - row /Increment Decrement Third Harmony Clear Fears And Dreams \\(Original Mix\\) Clear \\d+:\\d+:\\d+ \\d+:\\d+:\\d+ \\d+:\\d+:\\d+/:\n      - cell:\n        - checkbox\n      - cell \"Increment Decrement\":\n        - spinbutton: \"2\"\n        - button \"Increment\"\n        - button \"Decrement\"\n      - cell \"Third Harmony Clear\":\n        - textbox: Third Harmony\n        - button \"Clear\"\n        - button\n      - cell \"Fears And Dreams (Original Mix) Clear\":\n        - textbox: Fears And Dreams (Original Mix)\n        - button \"Clear\"\n        - button\n      - cell /\\d+:\\d+:\\d+/:\n        - textbox: /\\d+:\\d+:\\d+/\n      - cell /\\d+:\\d+:\\d+/:\n        - textbox: /\\d+:\\d+:\\d+/\n      - cell /\\d+:\\d+:\\d+/:\n        - textbox: /\\d+:\\d+:\\d+/\n    - row /Increment Decrement Dele Sosimi Afrobeat Orchestra Clear Too Much Information \\(Laolu Remix; Edit\\) Clear \\d+:\\d+:\\d+ \\d+:\\d+:\\d+ \\d+:\\d+:\\d+/:\n      - cell:\n        - checkbox\n      - cell \"Increment Decrement\":\n        - spinbutton: \"3\"\n        - button \"Increment\"\n        - button \"Decrement\"\n      - cell \"Dele Sosimi Afrobeat Orchestra Clear\":\n        - textbox: Dele Sosimi Afrobeat Orchestra\n        - button \"Clear\"\n        - button\n      - cell \"Too Much Information (Laolu Remix; Edit) Clear\":\n        - textbox: Too Much Information (Laolu Remix; Edit)\n        - button \"Clear\"\n        - button\n      - cell /\\d+:\\d+:\\d+/:\n        - textbox: /\\d+:\\d+:\\d+/\n      - cell /\\d+:\\d+:\\d+/:\n        - textbox: /\\d+:\\d+:\\d+/\n      - cell /\\d+:\\d+:\\d+/:\n        - textbox: /\\d+:\\d+:\\d+/\n    - row /Increment Decrement Edem, Govan Clear Ankh \\(Onetwo MX Remix\\) Clear \\d+:\\d+:\\d+ \\d+:\\d+:\\d+ \\d+:\\d+:\\d+/:\n      - cell:\n        - checkbox\n      - cell \"Increment Decrement\":\n        - spinbutton: \"4\"\n        - button \"Increment\"\n        - button \"Decrement\"\n      - cell \"Edem, Govan Clear\":\n        - textbox: Edem, Govan\n        - button \"Clear\"\n        - button\n      - cell \"Ankh (Onetwo MX Remix) Clear\":\n        - textbox: Ankh (Onetwo MX Remix)\n        - button \"Clear\"\n        - button\n      - cell /\\d+:\\d+:\\d+/:\n        - textbox: /\\d+:\\d+:\\d+/\n      - cell /\\d+:\\d+:\\d+/:\n        - textbox: /\\d+:\\d+:\\d+/\n      - cell /\\d+:\\d+:\\d+/:\n        - textbox: /\\d+:\\d+:\\d+/\n    - row /Increment Decrement Jody Wisternoff Clear For All Time \\(feat\\. Hendrik Burkhard\\) \\(Extended Mix\\) Clear \\d+:\\d+:\\d+ \\d+:\\d+:\\d+ \\d+:\\d+:\\d+/:\n      - cell:\n        - checkbox\n      - cell \"Increment Decrement\":\n        - spinbutton: \"5\"\n        - button \"Increment\"\n        - button \"Decrement\"\n      - cell \"Jody Wisternoff Clear\":\n        - textbox: Jody Wisternoff\n        - button \"Clear\"\n        - button\n      - cell \"For All Time (feat. Hendrik Burkhard) (Extended Mix) Clear\":\n        - textbox: For All Time (feat. Hendrik Burkhard) (Extended Mix)\n        - button \"Clear\"\n        - button\n      - cell /\\d+:\\d+:\\d+/:\n        - textbox: /\\d+:\\d+:\\d+/\n      - cell /\\d+:\\d+:\\d+/:\n        - textbox: /\\d+:\\d+:\\d+/\n      - cell /\\d+:\\d+:\\d+/:\n        - textbox: /\\d+:\\d+:\\d+/\n    - row /Increment Decrement Einmusik Clear Bead \\(Original Mix\\) Clear \\d+:\\d+:\\d+ \\d+:\\d+:\\d+ \\d+:\\d+:\\d+/:\n      - cell:\n        - checkbox\n      - cell \"Increment Decrement\":\n        - spinbutton: \"6\"\n        - button \"Increment\"\n        - button \"Decrement\"\n      - cell \"Einmusik Clear\":\n        - textbox: Einmusik\n        - button \"Clear\"\n        - button\n      - cell \"Bead (Original Mix) Clear\":\n        - textbox: Bead (Original Mix)\n        - button \"Clear\"\n        - button\n      - cell /\\d+:\\d+:\\d+/:\n        - textbox: /\\d+:\\d+:\\d+/\n      - cell /\\d+:\\d+:\\d+/:\n        - textbox: /\\d+:\\d+:\\d+/\n      - cell /\\d+:\\d+:\\d+/:\n        - textbox: /\\d+:\\d+:\\d+/\n    - row /Increment Decrement Sebastien Leger Clear La Danse du Scorpion Clear \\d+:\\d+:\\d+ \\d+:\\d+:\\d+ \\d+:\\d+:\\d+/:\n      - cell:\n        - checkbox\n      - cell \"Increment Decrement\":\n        - spinbutton: \"7\"\n        - button \"Increment\"\n        - button \"Decrement\"\n      - cell \"Sebastien Leger Clear\":\n        - textbox: Sebastien Leger\n        - button \"Clear\"\n        - button\n      - cell \"La Danse du Scorpion Clear\":\n        - textbox: La Danse du Scorpion\n        - button \"Clear\"\n        - button\n      - cell /\\d+:\\d+:\\d+/:\n        - textbox: /\\d+:\\d+:\\d+/\n      - cell /\\d+:\\d+:\\d+/:\n        - textbox: /\\d+:\\d+:\\d+/\n      - cell /\\d+:\\d+:\\d+/:\n        - textbox: /\\d+:\\d+:\\d+/\n    - row /Increment Decrement Paul Thomas & Solid Stone Clear La Bombo \\(Solid Stone Remix\\) Clear \\d+:\\d+:\\d+ \\d+:\\d+:\\d+ \\d+:\\d+:\\d+/:\n      - cell:\n        - checkbox\n      - cell \"Increment Decrement\":\n        - spinbutton: \"8\"\n        - button \"Increment\"\n        - button \"Decrement\"\n      - cell \"Paul Thomas & Solid Stone Clear\":\n        - textbox: Paul Thomas & Solid Stone\n        - button \"Clear\"\n        - button\n      - cell \"La Bombo (Solid Stone Remix) Clear\":\n        - textbox: La Bombo (Solid Stone Remix)\n        - button \"Clear\"\n        - button\n      - cell /\\d+:\\d+:\\d+/:\n        - textbox: /\\d+:\\d+:\\d+/\n      - cell /\\d+:\\d+:\\d+/:\n        - textbox: /\\d+:\\d+:\\d+/\n      - cell /\\d+:\\d+:\\d+/:\n        - textbox: /\\d+:\\d+:\\d+/\n    - row /Increment Decrement GusGus Clear Crossfade \\(Maceo Plex Mix\\) Clear \\d+:\\d+:\\d+ \\d+:\\d+:\\d+ \\d+:\\d+:\\d+/:\n      - cell:\n        - checkbox\n      - cell \"Increment Decrement\":\n        - spinbutton: \"9\"\n        - button \"Increment\"\n        - button \"Decrement\"\n      - cell \"GusGus Clear\":\n        - textbox: GusGus\n        - button \"Clear\"\n        - button\n      - cell \"Crossfade (Maceo Plex Mix) Clear\":\n        - textbox: Crossfade (Maceo Plex Mix)\n        - button \"Clear\"\n        - button\n      - cell /\\d+:\\d+:\\d+/:\n        - textbox: /\\d+:\\d+:\\d+/\n      - cell /\\d+:\\d+:\\d+/:\n        - textbox: /\\d+:\\d+:\\d+/\n      - cell /\\d+:\\d+:\\d+/:\n        - textbox: /\\d+:\\d+:\\d+/\n    - row /Increment Decrement Klangkarussell Clear Time \\(Original Mix\\) Clear \\d+:\\d+:\\d+ \\d+:\\d+:\\d+ \\d+:\\d+:\\d+/:\n      - cell:\n        - checkbox\n      - cell \"Increment Decrement\":\n        - spinbutton: /\\d+/\n        - button \"Increment\"\n        - button \"Decrement\"\n      - cell \"Klangkarussell Clear\":\n        - textbox: Klangkarussell\n        - button \"Clear\"\n        - button\n      - cell \"Time (Original Mix) Clear\":\n        - textbox: Time (Original Mix)\n        - button \"Clear\"\n        - button\n      - cell /\\d+:\\d+:\\d+/:\n        - textbox: /\\d+:\\d+:\\d+/\n      - cell /\\d+:\\d+:\\d+/:\n        - textbox: /\\d+:\\d+:\\d+/\n      - cell /\\d+:\\d+:\\d+/:\n        - textbox: /\\d+:\\d+:\\d+/\n    - row /Increment Decrement Anysense & Un:said Clear Missing Path \\(Original Mix\\) Clear \\d+:\\d+:\\d+ \\d+:\\d+:\\d+ \\d+:\\d+:\\d+/:\n      - cell:\n        - checkbox\n      - cell \"Increment Decrement\":\n        - spinbutton: /\\d+/\n        - button \"Increment\"\n        - button \"Decrement\"\n      - cell \"Anysense & Un:said Clear\":\n        - textbox: Anysense & Un:said\n        - button \"Clear\"\n        - button\n      - cell \"Missing Path (Original Mix) Clear\":\n        - textbox: Missing Path (Original Mix)\n        - button \"Clear\"\n        - button\n      - cell /\\d+:\\d+:\\d+/:\n        - textbox: /\\d+:\\d+:\\d+/\n      - cell /\\d+:\\d+:\\d+/:\n        - textbox: /\\d+:\\d+:\\d+/\n      - cell /\\d+:\\d+:\\d+/:\n        - textbox: /\\d+:\\d+:\\d+/\n    - row /Increment Decrement Space Food Clear Bombay Clear \\d+:\\d+:\\d+ \\d+:\\d+:\\d+ \\d+:\\d+:\\d+/:\n      - cell:\n        - checkbox\n      - cell \"Increment Decrement\":\n        - spinbutton: /\\d+/\n        - button \"Increment\"\n        - button \"Decrement\"\n      - cell \"Space Food Clear\":\n        - textbox: Space Food\n        - button \"Clear\"\n        - button\n      - cell \"Bombay Clear\":\n        - textbox: Bombay\n        - button \"Clear\"\n        - button\n      - cell /\\d+:\\d+:\\d+/:\n        - textbox: /\\d+:\\d+:\\d+/\n      - cell /\\d+:\\d+:\\d+/:\n        - textbox: /\\d+:\\d+:\\d+/\n      - cell /\\d+:\\d+:\\d+/:\n        - textbox: /\\d+:\\d+:\\d+/\n    - row /Increment Decrement SHDW & Obscure Shape Clear Wächter der Nacht \\(Original Mix\\) Clear \\d+:\\d+:\\d+ \\d+:\\d+:\\d+ \\d+:\\d+:\\d+/:\n      - cell:\n        - checkbox\n      - cell \"Increment Decrement\":\n        - spinbutton: /\\d+/\n        - button \"Increment\"\n        - button \"Decrement\"\n      - cell \"SHDW & Obscure Shape Clear\":\n        - textbox: SHDW & Obscure Shape\n        - button \"Clear\"\n        - button\n      - cell \"Wächter der Nacht (Original Mix) Clear\":\n        - textbox: Wächter der Nacht (Original Mix)\n        - button \"Clear\"\n        - button\n      - cell /\\d+:\\d+:\\d+/:\n        - textbox: /\\d+:\\d+:\\d+/\n      - cell /\\d+:\\d+:\\d+/:\n        - textbox: /\\d+:\\d+:\\d+/\n      - cell /\\d+:\\d+:\\d+/:\n        - textbox: /\\d+:\\d+:\\d+/\n    - row /Increment Decrement HOSH Clear Karma Clear \\d+:\\d+:\\d+ \\d+:\\d+:\\d+ \\d+:\\d+:\\d+/:\n      - cell:\n        - checkbox\n      - cell \"Increment Decrement\":\n        - spinbutton: /\\d+/\n        - button \"Increment\"\n        - button \"Decrement\"\n      - cell \"HOSH Clear\":\n        - textbox: HOSH\n        - button \"Clear\"\n        - button\n      - cell \"Karma Clear\":\n        - textbox: Karma\n        - button \"Clear\"\n        - button\n      - cell /\\d+:\\d+:\\d+/:\n        - textbox: /\\d+:\\d+:\\d+/\n      - cell /\\d+:\\d+:\\d+/:\n        - textbox: /\\d+:\\d+:\\d+/\n      - cell /\\d+:\\d+:\\d+/:\n        - textbox: /\\d+:\\d+:\\d+/\n    - row /Increment Decrement Alexey Union Clear Olympia \\(Original Mix\\) Clear \\d+:\\d+:\\d+ \\d+:\\d+:\\d+ \\d+:\\d+:\\d+/:\n      - cell:\n        - checkbox\n      - cell \"Increment Decrement\":\n        - spinbutton: /\\d+/\n        - button \"Increment\"\n        - button \"Decrement\"\n      - cell \"Alexey Union Clear\":\n        - textbox: Alexey Union\n        - button \"Clear\"\n        - button\n      - cell \"Olympia (Original Mix) Clear\":\n        - textbox: Olympia (Original Mix)\n        - button \"Clear\"\n        - button\n      - cell /\\d+:\\d+:\\d+/:\n        - textbox: /\\d+:\\d+:\\d+/\n      - cell /\\d+:\\d+:\\d+/:\n        - textbox: /\\d+:\\d+:\\d+/\n      - cell /\\d+:\\d+:\\d+/:\n        - textbox: /\\d+:\\d+:\\d+/\n    - row /Increment Decrement Paul Taylor Clear Afterglow Clear \\d+:\\d+:\\d+ \\d+:\\d+:\\d+ \\d+:\\d+:\\d+/:\n      - cell:\n        - checkbox\n      - cell \"Increment Decrement\":\n        - spinbutton: /\\d+/\n        - button \"Increment\"\n        - button \"Decrement\"\n      - cell \"Paul Taylor Clear\":\n        - textbox: Paul Taylor\n        - button \"Clear\"\n        - button\n      - cell \"Afterglow Clear\":\n        - textbox: Afterglow\n        - button \"Clear\"\n        - button\n      - cell /\\d+:\\d+:\\d+/:\n        - textbox: /\\d+:\\d+:\\d+/\n      - cell /\\d+:\\d+:\\d+/:\n        - textbox: /\\d+:\\d+:\\d+/\n      - cell /\\d+:\\d+:\\d+/:\n        - textbox: /\\d+:\\d+:\\d+/\n    - row /Increment Decrement Philter Clear Stranger Clear \\d+:\\d+:\\d+ \\d+:\\d+:\\d+ \\d+:\\d+:\\d+/:\n      - cell:\n        - checkbox\n      - cell \"Increment Decrement\":\n        - spinbutton: /\\d+/\n        - button \"Increment\"\n        - button \"Decrement\"\n      - cell \"Philter Clear\":\n        - textbox: Philter\n        - button \"Clear\"\n        - button\n      - cell \"Stranger Clear\":\n        - textbox: Stranger\n        - button \"Clear\"\n        - button\n      - cell /\\d+:\\d+:\\d+/:\n        - textbox: /\\d+:\\d+:\\d+/\n      - cell /\\d+:\\d+:\\d+/:\n        - textbox: /\\d+:\\d+:\\d+/\n      - cell /\\d+:\\d+:\\d+/:\n        - textbox: /\\d+:\\d+:\\d+/\n    - row /Increment Decrement Skizologic Clear Hypersphere \\(Original Mix\\) Clear \\d+:\\d+:\\d+ \\d+:\\d+:\\d+ \\d+:\\d+:\\d+/:\n      - cell:\n        - checkbox\n      - cell \"Increment Decrement\":\n        - spinbutton: /\\d+/\n        - button \"Increment\"\n        - button \"Decrement\"\n      - cell \"Skizologic Clear\":\n        - textbox: Skizologic\n        - button \"Clear\"\n        - button\n      - cell \"Hypersphere (Original Mix) Clear\":\n        - textbox: Hypersphere (Original Mix)\n        - button \"Clear\"\n        - button\n      - cell /\\d+:\\d+:\\d+/:\n        - textbox: /\\d+:\\d+:\\d+/\n      - cell /\\d+:\\d+:\\d+/:\n        - textbox: /\\d+:\\d+:\\d+/\n      - cell /\\d+:\\d+:\\d+/:\n        - textbox: /\\d+:\\d+:\\d+/\n    - row /Increment Decrement Thomas Schumacher, Caitlin Clear All of You \\(Remix\\) Clear \\d+:\\d+:\\d+ \\d+:\\d+:\\d+ \\d+:\\d+:\\d+/:\n      - cell:\n        - checkbox\n      - cell \"Increment Decrement\":\n        - spinbutton: /\\d+/\n        - button \"Increment\"\n        - button \"Decrement\"\n      - cell \"Thomas Schumacher, Caitlin Clear\":\n        - textbox: Thomas Schumacher, Caitlin\n        - button \"Clear\"\n        - button\n      - cell \"All of You (Remix) Clear\":\n        - textbox: All of You (Remix)\n        - button \"Clear\"\n        - button\n      - cell /\\d+:\\d+:\\d+/:\n        - textbox: /\\d+:\\d+:\\d+/\n      - cell /\\d+:\\d+:\\d+/:\n        - textbox: /\\d+:\\d+:\\d+/\n      - cell /\\d+:\\d+:\\d+/:\n        - textbox: /\\d+:\\d+:\\d+/\n    - row /Increment Decrement A\\. Skomoroh Clear White Horse Conquest \\(Original Mix\\) Clear \\d+:\\d+:\\d+ \\d+:\\d+:\\d+ \\d+:\\d+:\\d+/:\n      - cell:\n        - checkbox\n      - cell \"Increment Decrement\":\n        - spinbutton: /\\d+/\n        - button \"Increment\"\n        - button \"Decrement\"\n      - cell \"A. Skomoroh Clear\":\n        - textbox: A. Skomoroh\n        - button \"Clear\"\n        - button\n      - cell \"White Horse Conquest (Original Mix) Clear\":\n        - textbox: White Horse Conquest (Original Mix)\n        - button \"Clear\"\n        - button\n      - cell /\\d+:\\d+:\\d+/:\n        - textbox: /\\d+:\\d+:\\d+/\n      - cell /\\d+:\\d+:\\d+/:\n        - textbox: /\\d+:\\d+:\\d+/\n      - cell /\\d+:\\d+:\\d+/:\n        - textbox: /\\d+:\\d+:\\d+/\n    - row /Increment Decrement Patrik Berg Clear Bright \\(Original Mix\\) Clear \\d+:\\d+:\\d+ \\d+:\\d+:\\d+ \\d+:\\d+:\\d+/:\n      - cell:\n        - checkbox\n      - cell \"Increment Decrement\":\n        - spinbutton: /\\d+/\n        - button \"Increment\"\n        - button \"Decrement\"\n      - cell \"Patrik Berg Clear\":\n        - textbox: Patrik Berg\n        - button \"Clear\"\n        - button\n      - cell \"Bright (Original Mix) Clear\":\n        - textbox: Bright (Original Mix)\n        - button \"Clear\"\n        - button\n      - cell /\\d+:\\d+:\\d+/:\n        - textbox: /\\d+:\\d+:\\d+/\n      - cell /\\d+:\\d+:\\d+/:\n        - textbox: /\\d+:\\d+:\\d+/\n      - cell /\\d+:\\d+:\\d+/:\n        - textbox: /\\d+:\\d+:\\d+/\n    - row /Increment Decrement Hidden Empire Clear Bengal Clear \\d+:\\d+:\\d+ \\d+:\\d+:\\d+ \\d+:\\d+:\\d+/:\n      - cell:\n        - checkbox\n      - cell \"Increment Decrement\":\n        - spinbutton: /\\d+/\n        - button \"Increment\"\n        - button \"Decrement\"\n      - cell \"Hidden Empire Clear\":\n        - textbox: Hidden Empire\n        - button \"Clear\"\n        - button\n      - cell \"Bengal Clear\":\n        - textbox: Bengal\n        - button \"Clear\"\n        - button\n      - cell /\\d+:\\d+:\\d+/:\n        - textbox: /\\d+:\\d+:\\d+/\n      - cell /\\d+:\\d+:\\d+/:\n        - textbox: /\\d+:\\d+:\\d+/\n      - cell /\\d+:\\d+:\\d+/:\n        - textbox: /\\d+:\\d+:\\d+/\n    - row /Increment Decrement Mario Ochoa Clear Levitate Clear \\d+:\\d+:\\d+ \\d+:\\d+:\\d+ \\d+:\\d+:\\d+/:\n      - cell:\n        - checkbox\n      - cell \"Increment Decrement\":\n        - spinbutton: /\\d+/\n        - button \"Increment\"\n        - button \"Decrement\"\n      - cell \"Mario Ochoa Clear\":\n        - textbox: Mario Ochoa\n        - button \"Clear\"\n        - button\n      - cell \"Levitate Clear\":\n        - textbox: Levitate\n        - button \"Clear\"\n        - button\n      - cell /\\d+:\\d+:\\d+/:\n        - textbox: /\\d+:\\d+:\\d+/\n      - cell /\\d+:\\d+:\\d+/:\n        - textbox: /\\d+:\\d+:\\d+/\n      - cell /\\d+:\\d+:\\d+/:\n        - textbox: /\\d+:\\d+:\\d+/\n    - row /Increment Decrement Raul Facio Clear Eyes Wide Shut \\(Original Mix\\) Clear \\d+:\\d+:\\d+ \\d+:\\d+:\\d+ \\d+:\\d+:\\d+/:\n      - cell:\n        - checkbox\n      - cell \"Increment Decrement\":\n        - spinbutton: /\\d+/\n        - button \"Increment\"\n        - button \"Decrement\"\n      - cell \"Raul Facio Clear\":\n        - textbox: Raul Facio\n        - button \"Clear\"\n        - button\n      - cell \"Eyes Wide Shut (Original Mix) Clear\":\n        - textbox: Eyes Wide Shut (Original Mix)\n        - button \"Clear\"\n        - button\n      - cell /\\d+:\\d+:\\d+/:\n        - textbox: /\\d+:\\d+:\\d+/\n      - cell /\\d+:\\d+:\\d+/:\n        - textbox: /\\d+:\\d+:\\d+/\n      - cell /\\d+:\\d+:\\d+/:\n        - textbox: /\\d+:\\d+:\\d+/\n    - row /Increment Decrement Soolver Clear Regular \\(Original Mix\\) Clear \\d+:\\d+:\\d+ \\d+:\\d+:\\d+ \\d+:\\d+:\\d+/:\n      - cell:\n        - checkbox\n      - cell \"Increment Decrement\":\n        - spinbutton: /\\d+/\n        - button \"Increment\"\n        - button \"Decrement\"\n      - cell \"Soolver Clear\":\n        - textbox: Soolver\n        - button \"Clear\"\n        - button\n      - cell \"Regular (Original Mix) Clear\":\n        - textbox: Regular (Original Mix)\n        - button \"Clear\"\n        - button\n      - cell /\\d+:\\d+:\\d+/:\n        - textbox: /\\d+:\\d+:\\d+/\n      - cell /\\d+:\\d+:\\d+/:\n        - textbox: /\\d+:\\d+:\\d+/\n      - cell /\\d+:\\d+:\\d+/:\n        - textbox: /\\d+:\\d+:\\d+/\n    - row /Increment Decrement Weska Clear EQ64 \\(Original Mix\\) Clear \\d+:\\d+:\\d+ \\d+:\\d+:\\d+ \\d+:\\d+:\\d+/:\n      - cell:\n        - checkbox\n      - cell \"Increment Decrement\":\n        - spinbutton: /\\d+/\n        - button \"Increment\"\n        - button \"Decrement\"\n      - cell \"Weska Clear\":\n        - textbox: Weska\n        - button \"Clear\"\n        - button\n      - cell \"EQ64 (Original Mix) Clear\":\n        - textbox: EQ64 (Original Mix)\n        - button \"Clear\"\n        - button\n      - cell /\\d+:\\d+:\\d+/:\n        - textbox: /\\d+:\\d+:\\d+/\n      - cell /\\d+:\\d+:\\d+/:\n        - textbox: /\\d+:\\d+:\\d+/\n      - cell /\\d+:\\d+:\\d+/:\n        - textbox: /\\d+:\\d+:\\d+/\n    - row /Increment Decrement Tempo Giusto Clear The Fall \\(Extended Mix\\) Clear \\d+:\\d+:\\d+ \\d+:\\d+:\\d+ \\d+:\\d+:\\d+/:\n      - cell:\n        - checkbox\n      - cell \"Increment Decrement\":\n        - spinbutton: /\\d+/\n        - button \"Increment\"\n        - button \"Decrement\"\n      - cell \"Tempo Giusto Clear\":\n        - textbox: Tempo Giusto\n        - button \"Clear\"\n        - button\n      - cell \"The Fall (Extended Mix) Clear\":\n        - textbox: The Fall (Extended Mix)\n        - button \"Clear\"\n        - button\n      - cell /\\d+:\\d+:\\d+/:\n        - textbox: /\\d+:\\d+:\\d+/\n      - cell /\\d+:\\d+:\\d+/:\n        - textbox: /\\d+:\\d+:\\d+/\n      - cell /\\d+:\\d+:\\d+/:\n        - textbox: /\\d+:\\d+:\\d+/\n    - row /Increment Decrement Vlind & Asteroid & Gary Leroy Clear Trinity \\(Extended Mix\\) Clear \\d+:\\d+:\\d+ \\d+:\\d+:\\d+ \\d+:\\d+:\\d+/:\n      - cell:\n        - checkbox\n      - cell \"Increment Decrement\":\n        - spinbutton: /\\d+/\n        - button \"Increment\"\n        - button \"Decrement\"\n      - cell \"Vlind & Asteroid & Gary Leroy Clear\":\n        - textbox: Vlind & Asteroid & Gary Leroy\n        - button \"Clear\"\n        - button\n      - cell \"Trinity (Extended Mix) Clear\":\n        - textbox: Trinity (Extended Mix)\n        - button \"Clear\"\n        - button\n      - cell /\\d+:\\d+:\\d+/:\n        - textbox: /\\d+:\\d+:\\d+/\n      - cell /\\d+:\\d+:\\d+/:\n        - textbox: /\\d+:\\d+:\\d+/\n      - cell /\\d+:\\d+:\\d+/:\n        - textbox: /\\d+:\\d+:\\d+/\n    - row /Increment Decrement Astral Legacy Clear Vaveyla \\(Original Mix\\) Clear \\d+:\\d+:\\d+ \\d+:\\d+:\\d+ \\d+:\\d+:\\d+/:\n      - cell:\n        - checkbox\n      - cell \"Increment Decrement\":\n        - spinbutton: /\\d+/\n        - button \"Increment\"\n        - button \"Decrement\"\n      - cell \"Astral Legacy Clear\":\n        - textbox: Astral Legacy\n        - button \"Clear\"\n        - button\n      - cell \"Vaveyla (Original Mix) Clear\":\n        - textbox: Vaveyla (Original Mix)\n        - button \"Clear\"\n        - button\n      - cell /\\d+:\\d+:\\d+/:\n        - textbox: /\\d+:\\d+:\\d+/\n      - cell /\\d+:\\d+:\\d+/:\n        - textbox: /\\d+:\\d+:\\d+/\n      - cell /\\d+:\\d+:\\d+/:\n        - textbox: /\\d+:\\d+:\\d+/\n    - row /Increment Decrement Gerrox Clear Chakra \\(Original Mix\\) Clear \\d+:\\d+:\\d+ \\d+:\\d+:\\d+ \\d+:\\d+:\\d+/:\n      - cell:\n        - checkbox\n      - cell \"Increment Decrement\":\n        - spinbutton: /\\d+/\n        - button \"Increment\"\n        - button \"Decrement\"\n      - cell \"Gerrox Clear\":\n        - textbox: Gerrox\n        - button \"Clear\"\n        - button\n      - cell \"Chakra (Original Mix) Clear\":\n        - textbox: Chakra (Original Mix)\n        - button \"Clear\"\n        - button\n      - cell /\\d+:\\d+:\\d+/:\n        - textbox: /\\d+:\\d+:\\d+/\n      - cell /\\d+:\\d+:\\d+/:\n        - textbox: /\\d+:\\d+:\\d+/\n      - cell /\\d+:\\d+:\\d+/:\n        - textbox: /\\d+:\\d+:\\d+/\n    - row /Increment Decrement Charlotte De Witte Clear Pattern Clear \\d+:\\d+:\\d+ \\d+:\\d+:\\d+ \\d+:\\d+:\\d+/:\n      - cell:\n        - checkbox\n      - cell \"Increment Decrement\":\n        - spinbutton: /\\d+/\n        - button \"Increment\"\n        - button \"Decrement\"\n      - cell \"Charlotte De Witte Clear\":\n        - textbox: Charlotte De Witte\n        - button \"Clear\"\n        - button\n      - cell \"Pattern Clear\":\n        - textbox: Pattern\n        - button \"Clear\"\n        - button\n      - cell /\\d+:\\d+:\\d+/:\n        - textbox: /\\d+:\\d+:\\d+/\n      - cell /\\d+:\\d+:\\d+/:\n        - textbox: /\\d+:\\d+:\\d+/\n      - cell /\\d+:\\d+:\\d+/:\n        - textbox: /\\d+:\\d+:\\d+/\n    - row /Increment Decrement Space Food Clear Amabey Clear \\d+:\\d+:\\d+ \\d+:\\d+:\\d+ \\d+:\\d+:\\d+/:\n      - cell:\n        - checkbox\n      - cell \"Increment Decrement\":\n        - spinbutton: /\\d+/\n        - button \"Increment\"\n        - button \"Decrement\"\n      - cell \"Space Food Clear\":\n        - textbox: Space Food\n        - button \"Clear\"\n        - button\n      - cell \"Amabey Clear\":\n        - textbox: Amabey\n        - button \"Clear\"\n        - button\n      - cell /\\d+:\\d+:\\d+/:\n        - textbox: /\\d+:\\d+:\\d+/\n      - cell /\\d+:\\d+:\\d+/:\n        - textbox: /\\d+:\\d+:\\d+/\n      - cell /\\d+:\\d+:\\d+/:\n        - textbox: /\\d+:\\d+:\\d+/\n    - row /Increment Decrement ARTBAT Clear Papilion \\(Original Mix\\) Clear \\d+:\\d+:\\d+ \\d+:\\d+:\\d+ \\d+:\\d+:\\d+/:\n      - cell:\n        - checkbox\n      - cell \"Increment Decrement\":\n        - spinbutton: /\\d+/\n        - button \"Increment\"\n        - button \"Decrement\"\n      - cell \"ARTBAT Clear\":\n        - textbox: ARTBAT\n        - button \"Clear\"\n        - button\n      - cell \"Papilion (Original Mix) Clear\":\n        - textbox: Papilion (Original Mix)\n        - button \"Clear\"\n        - button\n      - cell /\\d+:\\d+:\\d+/:\n        - textbox: /\\d+:\\d+:\\d+/\n      - cell /\\d+:\\d+:\\d+/:\n        - textbox: /\\d+:\\d+:\\d+/\n      - cell /\\d+:\\d+:\\d+/:\n        - textbox: /\\d+:\\d+:\\d+/\n    - row /Increment Decrement PETER PAHN Clear Enjoy Infinity \\(Original Mix\\) Clear \\d+:\\d+:\\d+ \\d+:\\d+:\\d+ \\d+:\\d+:\\d+/:\n      - cell:\n        - checkbox\n      - cell \"Increment Decrement\":\n        - spinbutton: /\\d+/\n        - button \"Increment\"\n        - button \"Decrement\"\n      - cell \"PETER PAHN Clear\":\n        - textbox: PETER PAHN\n        - button \"Clear\"\n        - button\n      - cell \"Enjoy Infinity (Original Mix) Clear\":\n        - textbox: Enjoy Infinity (Original Mix)\n        - button \"Clear\"\n        - button\n      - cell /\\d+:\\d+:\\d+/:\n        - textbox: /\\d+:\\d+:\\d+/\n      - cell /\\d+:\\d+:\\d+/:\n        - textbox: /\\d+:\\d+:\\d+/\n      - cell /\\d+:\\d+:\\d+/:\n        - textbox: /\\d+:\\d+:\\d+/\n    - row /Increment Decrement Solitek Clear Instinct \\(Original Mix\\) Clear \\d+:\\d+:\\d+ \\d+:\\d+:\\d+ \\d+:\\d+:\\d+/:\n      - cell:\n        - checkbox\n      - cell \"Increment Decrement\":\n        - spinbutton: /\\d+/\n        - button \"Increment\"\n        - button \"Decrement\"\n      - cell \"Solitek Clear\":\n        - textbox: Solitek\n        - button \"Clear\"\n        - button\n      - cell \"Instinct (Original Mix) Clear\":\n        - textbox: Instinct (Original Mix)\n        - button \"Clear\"\n        - button\n      - cell /\\d+:\\d+:\\d+/:\n        - textbox: /\\d+:\\d+:\\d+/\n      - cell /\\d+:\\d+:\\d+/:\n        - textbox: /\\d+:\\d+:\\d+/\n      - cell /\\d+:\\d+:\\d+/:\n        - textbox: /\\d+:\\d+:\\d+/\n    - row /Increment Decrement Veerus Clear Heavy Clear \\d+:\\d+:\\d+ \\d+:\\d+:\\d+ \\d+:\\d+:\\d+/:\n      - cell:\n        - checkbox\n      - cell \"Increment Decrement\":\n        - spinbutton: /\\d+/\n        - button \"Increment\"\n        - button \"Decrement\"\n      - cell \"Veerus Clear\":\n        - textbox: Veerus\n        - button \"Clear\"\n        - button\n      - cell \"Heavy Clear\":\n        - textbox: Heavy\n        - button \"Clear\"\n        - button\n      - cell /\\d+:\\d+:\\d+/:\n        - textbox: /\\d+:\\d+:\\d+/\n      - cell /\\d+:\\d+:\\d+/:\n        - textbox: /\\d+:\\d+:\\d+/\n      - cell /\\d+:\\d+:\\d+/:\n        - textbox: /\\d+:\\d+:\\d+/\n    - row /Increment Decrement Secret Cinema & Reinier Zonneveld Clear Pain Thing \\(Original Mix\\) Clear \\d+:\\d+:\\d+ \\d+:\\d+:\\d+ \\d+:\\d+:\\d+/:\n      - cell:\n        - checkbox\n      - cell \"Increment Decrement\":\n        - spinbutton: /\\d+/\n        - button \"Increment\"\n        - button \"Decrement\"\n      - cell \"Secret Cinema & Reinier Zonneveld Clear\":\n        - textbox: Secret Cinema & Reinier Zonneveld\n        - button \"Clear\"\n        - button\n      - cell \"Pain Thing (Original Mix) Clear\":\n        - textbox: Pain Thing (Original Mix)\n        - button \"Clear\"\n        - button\n      - cell /\\d+:\\d+:\\d+/:\n        - textbox: /\\d+:\\d+:\\d+/\n      - cell /\\d+:\\d+:\\d+/:\n        - textbox: /\\d+:\\d+:\\d+/\n      - cell /\\d+:\\d+:\\d+/:\n        - textbox: /\\d+:\\d+:\\d+/\n    - row /Increment Decrement Amelie Lens Clear Hypnotized Clear \\d+:\\d+:\\d+ \\d+:\\d+:\\d+ \\d+:\\d+:\\d+/:\n      - cell:\n        - checkbox\n      - cell \"Increment Decrement\":\n        - spinbutton: /\\d+/\n        - button \"Increment\"\n        - button \"Decrement\"\n      - cell \"Amelie Lens Clear\":\n        - textbox: Amelie Lens\n        - button \"Clear\"\n        - button\n      - cell \"Hypnotized Clear\":\n        - textbox: Hypnotized\n        - button \"Clear\"\n        - button\n      - cell /\\d+:\\d+:\\d+/:\n        - textbox: /\\d+:\\d+:\\d+/\n      - cell /\\d+:\\d+:\\d+/:\n        - textbox: /\\d+:\\d+:\\d+/\n      - cell /\\d+:\\d+:\\d+/:\n        - textbox: /\\d+:\\d+:\\d+/\n    - row /Increment Decrement Nikolay Kirov Clear Chasing the Sun \\(Original Mix\\) Clear \\d+:\\d+:\\d+/:\n      - cell:\n        - checkbox\n      - cell \"Increment Decrement\":\n        - spinbutton: /\\d+/\n        - button \"Increment\"\n        - button \"Decrement\"\n      - cell \"Nikolay Kirov Clear\":\n        - textbox: Nikolay Kirov\n        - button \"Clear\"\n        - button\n      - cell \"Chasing the Sun (Original Mix) Clear\":\n        - textbox: Chasing the Sun (Original Mix)\n        - button \"Clear\"\n        - button\n      - cell /\\d+:\\d+:\\d+/:\n        - textbox: /\\d+:\\d+:\\d+/\n      - cell:\n        - textbox\n      - cell:\n        - textbox\n  - rowgroup:\n    - row");
        }

        [TestMethod]
        public async Task ImportTestSampleInputfile2Async()
        {
            await Page.GotoAsync("http://localhost:5132/");
            await Page.GetByText("Import view").ClickAsync();
            await Page.GetByRole(AriaRole.Button, new() { Name = "Choose File" }).SetInputFilesAsync(new[] { "Sample_Inputfile2.txt" });
            await Page.GetByRole(AriaRole.Group).Filter(new() { HasText = "Scheme common dataScheme" }).GetByLabel("Clear").ClickAsync();
            await Expect(Page.GetByRole(AriaRole.Textbox, new() { Name = "Cuesheet artist" })).ToBeEmptyAsync();
            await Expect(Page.GetByRole(AriaRole.Textbox, new() { Name = "Cuesheet title" })).ToBeEmptyAsync();
            await Expect(Page.GetByRole(AriaRole.Cell, new() { Name = "Sample Title 1 Clear" })).ToBeVisibleAsync();
            await Expect(Page.GetByRole(AriaRole.Cell, new() { Name = ":09:23" }).Nth(1)).ToBeVisibleAsync();
            await Expect(Page.GetByRole(AriaRole.Cell, new() { Name = "01:15:" })).ToBeVisibleAsync();
            await Expect(Page.GetByRole(AriaRole.Cell, new() { Name = "Sample Artist 8 Clear" })).ToBeVisibleAsync();
        }

        [TestMethod]
        public async Task ImportTestTraktorAsync()
        {
            await Page.GotoAsync("http://localhost:5132/");
            await Page.GetByText("Import view").ClickAsync();
            await Page.GetByRole(AriaRole.Button, new() { Name = "Choose File" }).SetInputFilesAsync(new[] { "Traktor Export.html" });
            await Page.GetByText("Textfile (common data in").ClickAsync();
            await Page.GetByText("Traktor history").ClickAsync();
            await Expect(Page.GetByRole(AriaRole.Cell, new() { Name = "Arba Han Clear" })).ToBeVisibleAsync();
            await Expect(Page.GetByRole(AriaRole.Cell, new() { Name = ":48:53" }).Nth(1)).ToBeVisibleAsync();
            await Expect(Page.GetByRole(AriaRole.Cell, new() { Name = "Acid Rain Clear" })).ToBeVisibleAsync();
            await Expect(Page.GetByRole(AriaRole.Cell, new() { Name = ":48:40" }).Nth(1)).ToBeVisibleAsync();
            await Page.GetByRole(AriaRole.Button, new() { Name = "Complete" }).ClickAsync();
            await Expect(Page.GetByRole(AriaRole.Cell, new() { Name = "Space Yoda (Snyl Remix) Clear" })).ToBeVisibleAsync();
        }
    }
}
