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
using AudioCuesheetEditor.End2EndTests.Models;
using Microsoft.Playwright;
using System.Xml.Linq;
using static System.Net.Mime.MediaTypeNames;

namespace AudioCuesheetEditor.End2EndTests.Tests.Desktop
{
    [TestClass]
    public class BasicTest : PlaywrightTestBase
    {
        [TestMethod]
        public async Task Application_HasTitle_WhenBrowsingIndex()
        {
            var detailView = new DetailView(TestPage, DeviceName != null);
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

        [TestMethod]
        public async Task Audiofile_ShouldBeRenamed_WhenEditingFilename()
        {
            var detailView = new DetailView(TestPage, DeviceName != null);
            await detailView.GotoAsync();
            await detailView.AudiofileInput.SetInputFilesAsync("Kalimba.mp3");
            await detailView.RenameAudiofileAsync("Kalimba test 123.mp3");
            await Expect(TestPage.GetByRole(AriaRole.Textbox, new() { Name = "Audiofile" })).ToMatchAriaSnapshotAsync("- textbox \"Audiofile\": Kalimba test 123.mp3");
        }

        [TestMethod]
        public async Task OpenSettings_ShouldDisplaySettings_WhenSelectingSettings()
        {
            var bar = new AppBar(TestPage);
            var detailView = new DetailView(TestPage, DeviceName != null);
            await detailView.GotoAsync();
            await bar.OpenSettingsAsync();
            await Expect(TestPage.GetByRole(AriaRole.Heading, new() { Name = "Settings" })).ToBeVisibleAsync();
        }

        [TestMethod]
        public async Task ChangeLanguage_ShouldSwitchLanguage_WhenGermanIsSelected()
        {
            var bar = new AppBar(TestPage);
            var exportDialog = new ExportDialog(TestPage);
            var detailView = new DetailView(TestPage, DeviceName != null);
            await detailView.GotoAsync();
            await bar.ChangeLanguageAsync("German (Germany)");
            await Expect(TestPage.GetByRole(AriaRole.Heading, new() { Name = "Abschnitte" })).ToBeVisibleAsync();
            await Expect(TestPage.GetByRole(AriaRole.Heading, new() { Name = "Allgemeine Informationen" })).ToBeVisibleAsync();
            await Expect(TestPage.GetByText("Aufnahmeansicht")).ToBeVisibleAsync();
            await Expect(TestPage.GetByRole(AriaRole.Heading, new() { Name = "Titel" })).ToBeVisibleAsync();
            await Expect(TestPage.GetByRole(AriaRole.Heading, new() { Name = "Wiedergabe" })).ToBeVisibleAsync();
            await bar.OpenExportDialogAsync("Textdatei", "Datei");
            await Expect(TestPage.GetByRole(AriaRole.Dialog)).ToMatchAriaSnapshotAsync("- tabpanel:\n  - text: \"Export ist derzeit nicht möglich: Titel hat ungültige Anzahl (0)! Künstler hat keinen Wert! Titel hat keinen Wert! Audiodatei hat keinen Wert! YouTube\"\n  - group \"Exportprofil auswählen\"\n  - text: Exportprofil auswählen\n  - group:\n    - button \"Neues Exportprofil hinzufügen\"\n    - button \"Ausgewähltes Exportprofil löschen\"\n  - separator\n  - textbox \"Name\": YouTube\n  - group \"Name\"\n  - text: Name\n  - textbox \"Dateiname\": YouTube.txt\n  - group \"Dateiname\"\n  - text: Dateiname\n  - textbox \"Schema Kopf\": \"%Cuesheet.Artist% - %Cuesheet.Title%\"\n  - button \"Clear\"\n  - button\n  - group \"Schema Kopf\"\n  - text: Schema Kopf\n  - textbox \"Schema Titel\": \"%Track.Artist% - %Track.Title% %Track.Begin%\"\n  - button \"Clear\"\n  - button\n  - group \"Schema Titel\"\n  - text: Schema Titel\n  - textbox \"Schema Fuß\"\n  - button\n  - group \"Schema Fuß\"\n  - text: Schema Fuß");
            await exportDialog.OpenSchemeMenuAsync("Schema Kopf");
            await Expect(TestPage.Locator("#app")).ToMatchAriaSnapshotAsync("- paragraph: Künstler\n- paragraph: Titel\n- paragraph: Audiodatei\n- paragraph: CDTextdatei\n- paragraph: Katalognummer\n- paragraph: Datum\n- paragraph: Datum & Uhrzeit\n- paragraph: Uhrzeit");
            await TestPage.GetByText("CDTextdatei").ClickAsync();
            await exportDialog.OpenSchemeMenuAsync("Schema Titel");
            await Expect(TestPage.Locator("#app")).ToMatchAriaSnapshotAsync("- paragraph: Position\n- paragraph: Künstler\n- paragraph: Titel\n- paragraph: Beginn\n- paragraph: Ende\n- paragraph: Länge\n- paragraph: Markierungen\n- paragraph: Vorlücke\n- paragraph: Nachlücke");
        }

        [TestMethod]
        public async Task TrackTableControls_ShouldBeEnabled_WhenSelectingFirstTrackAsync()
        {
            var bar = new AppBar(TestPage);
            var detailView = new DetailView(TestPage, DeviceName != null);
            await detailView.GotoAsync();
            await detailView.AddTrackAsync();
            await detailView.AddTrackAsync();
            await detailView.SelectTracksAsync([1]);
            await bar.ChangeLanguageAsync("German (Germany)");
            await Expect(TestPage.GetByLabel("Track table controls")).ToMatchAriaSnapshotAsync(@"- group:
  - button ""Neuen Titel hinzufügen""
  - button ""Ausgewählte Titel bearbeiten""
  - button ""Ausgewählten Titel kopieren""
  - button ""Ausgewählte Titel löschen""
  - button ""Alle Titel löschen""
- group:
  - button ""Ausgewählte Titel nach oben bewegen"" [disabled]
  - button ""Ausgewählte Titel nach unten bewegen""
- button ""Fester Tabellenkopf""");
        }

        [TestMethod]
        public async Task KeyboardCommands_ShouldControlDialogs_WhenUsingEnterOrEscapeAsync()
        {
            var bar = new AppBar(TestPage);
            var detailView = new DetailView(TestPage, DeviceName != null);
            await detailView.GotoAsync();
            await bar.OpenFileDialogAsync();
            await Expect(TestPage.GetByRole(AriaRole.Dialog)).ToBeVisibleAsync();
            await TestPage.Keyboard.PressAsync("Escape");
            await TestPage.GetByRole(AriaRole.Dialog).WaitForAsync(new() { State = WaitForSelectorState.Detached });
            await bar.OpenExportDialogAsync("Cuesheet");
            await Expect(TestPage.GetByRole(AriaRole.Dialog)).ToBeVisibleAsync();
            await TestPage.Keyboard.PressAsync("Escape");
            await TestPage.GetByRole(AriaRole.Dialog).WaitForAsync(new() { State = WaitForSelectorState.Detached });
            await bar.OpenExportDialogAsync("Projectfile");
            await Expect(TestPage.GetByRole(AriaRole.Dialog)).ToBeVisibleAsync();
            await TestPage.Keyboard.PressAsync("Escape");
            await TestPage.GetByRole(AriaRole.Dialog).WaitForAsync(new() { State = WaitForSelectorState.Detached });
            await bar.OpenExportDialogAsync("Textfile");
            await Expect(TestPage.GetByRole(AriaRole.Dialog)).ToBeVisibleAsync();
            await TestPage.Keyboard.PressAsync("Escape");
            await TestPage.GetByRole(AriaRole.Dialog).WaitForAsync(new() { State = WaitForSelectorState.Detached });
            await bar.OpenSettingsAsync();
            await Expect(TestPage.GetByRole(AriaRole.Dialog)).ToBeVisibleAsync();
            await TestPage.Keyboard.PressAsync("Escape");
            await TestPage.GetByRole(AriaRole.Dialog).WaitForAsync(new() { State = WaitForSelectorState.Detached });
            await bar.OpenDisplayHotkeysAsync();
            await Expect(TestPage.GetByRole(AriaRole.Dialog)).ToBeVisibleAsync();
            await TestPage.Keyboard.PressAsync("Escape");
            await TestPage.GetByRole(AriaRole.Dialog).WaitForAsync(new() { State = WaitForSelectorState.Detached });
            await detailView.AudiofileInput.SetInputFilesAsync("Kalimba.mp3");
            await detailView.OpenRenameAudiofileDialogAsync();
            await Expect(TestPage.GetByRole(AriaRole.Dialog)).ToBeVisibleAsync();
            await detailView.NewFileNameInput.FillAsync("Test 123");
            await TestPage.Keyboard.PressAsync("Enter");
            await TestPage.GetByRole(AriaRole.Dialog).WaitForAsync(new() { State = WaitForSelectorState.Detached });
            await Expect(TestPage.GetByRole(AriaRole.Textbox, new() { Name = "Audiofile" })).ToMatchAriaSnapshotAsync("- textbox \"Audiofile\": Test 123.mp3");
        }
    }
}
