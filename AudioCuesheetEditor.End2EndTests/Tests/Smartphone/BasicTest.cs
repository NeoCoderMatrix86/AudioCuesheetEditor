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

namespace AudioCuesheetEditor.End2EndTests.Tests.Smartphone
{
    [TestClass]
    public class BasicTest : PlaywrightTestBase
    {
        protected override string? DeviceName => "iPhone 13";

        [TestMethod]
        public async Task Application_HasTitle_WhenBrowsingIndex()
        {
            var detailView = new DetailView(TestPage);
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
            var detailView = new DetailView(TestPage);
            await detailView.GotoAsync();
            await detailView.AddAudiofileAsync();
            await detailView.SetAudiofileInputFileAsync(0, "Kalimba.mp3");
            await detailView.RenameAudiofileAsync(0, "Kalimba test 123.mp3");
            await Expect(TestPage.GetByRole(AriaRole.Textbox, new() { Name = "Audiofile" })).ToMatchAriaSnapshotAsync("- textbox \"Audiofile\": Kalimba test 123.mp3");
        }

        [TestMethod]
        public async Task OpenSettings_ShouldDisplaySettings_WhenSelectingSettings()
        {
            var bar = new AppBar(TestPage);
            var detailView = new DetailView(TestPage);
            await detailView.GotoAsync();
            await bar.OpenSettingsAsync();
            await Expect(TestPage.GetByRole(AriaRole.Heading, new() { Name = "Settings" })).ToBeVisibleAsync();
        }

        [TestMethod]
        public async Task ChangeLanguage_ShouldSwitchLanguage_WhenGermanIsSelected()
        {
            var bar = new AppBar(TestPage);
            var exportDialog = new ExportDialog(TestPage);
            var detailView = new DetailView(TestPage);
            await detailView.GotoAsync();
            await bar.ChangeLanguageAsync("German (Germany)");
            await Expect(TestPage.GetByRole(AriaRole.Heading, new() { Name = "Allgemeine Informationen" })).ToBeVisibleAsync();
            await Expect(TestPage.GetByText("Aufnahmeansicht")).ToBeVisibleAsync();
            await Expect(TestPage.GetByRole(AriaRole.Heading, new() { Name = "Dateien" })).ToBeVisibleAsync();
            await Expect(TestPage.GetByRole(AriaRole.Heading, new() { Name = "Wiedergabe" })).ToBeVisibleAsync();
            await bar.OpenExportDialogAsync("Textdatei", "Datei");
            await Expect(TestPage.GetByRole(AriaRole.Dialog)).ToMatchAriaSnapshotAsync(@"- dialog ""Exportprofile Close"":
  - heading ""Exportprofile"" [level=6]
  - button ""Close""
  - tablist:
    - tab ""Export konfigurieren"" [selected]:
      - paragraph: Export konfigurieren
    - tab ""2 Export herunterladen"" [disabled]:
      - text: ""2""
      - paragraph: Export herunterladen
  - tabpanel ""Export konfigurieren"":
    - text: ""Export ist derzeit nicht möglich: Künstler hat keinen Wert! Titel hat keinen Wert! Audiodateien hat ungültige Anzahl (0)!""
    - combobox ""Exportprofil auswählen"": YouTube
    - group ""Exportprofil auswählen""
    - text: Exportprofil auswählen
    - group:
      - button ""Neues Exportprofil hinzufügen""
      - button ""Ausgewähltes Exportprofil löschen""
    - separator
    - textbox ""Name"":
      - /placeholder: Geben Sie hier den Namen für dieses Profil ein
      - text: YouTube
    - group ""Name""
    - text: Name
    - textbox ""Dateiname"":
      - /placeholder: Geben Sie hier den Dateinamen für dieses Profil ein
      - text: YouTube.txt
    - group ""Dateiname""
    - text: Dateiname
    - textbox ""Schema Kopf"":
      - /placeholder: Geben Sie hier das Kopf-Schema für dieses Profil ein
      - text: ""%Cuesheet.Artist% - %Cuesheet.Title%""
    - button ""Clear""
    - button
    - group ""Schema Kopf""
    - text: Schema Kopf
    - textbox ""Schema Audiodateien"":
      - /placeholder: Geben Sie hier das Audiodatei-Schema für dieses Profil ein
    - button
    - group ""Schema Audiodateien""
    - text: Schema Audiodateien
    - textbox ""Schema Titel"":
      - /placeholder: Geben Sie hier das Titel-Schema für dieses Profil ein
      - text: ""%Track.Artist% - %Track.Title% %Track.Begin%""
    - button ""Clear""
    - button
    - group ""Schema Titel""
    - text: Schema Titel
    - textbox ""Schema Fuß"":
      - /placeholder: Geben Sie hier das Fuß-Schema für dieses Profil ein
    - button
    - group ""Schema Fuß""
    - text: Schema Fuß
  - button ""Previous"" [disabled]
  - button ""Next"" [disabled]");
            await exportDialog.OpenSchemeMenuAsync("Schema Kopf");
            await Expect(TestPage.Locator("#app")).ToMatchAriaSnapshotAsync("- paragraph: Künstler\n- paragraph: Titel\n- paragraph: CDTextdatei\n- paragraph: Katalognummer\n- paragraph: Datum\n- paragraph: Datum & Uhrzeit\n- paragraph: Uhrzeit");
            await TestPage.GetByText("CDTextdatei").ClickAsync();
            await exportDialog.OpenSchemeMenuAsync("Schema Titel");
            await Expect(TestPage.GetByTestId("menu-wrapper")).ToMatchAriaSnapshotAsync("- paragraph: Position\n- paragraph: Künstler\n- paragraph: Titel\n- paragraph: Begin\n- paragraph: End\n- paragraph: Länge\n- paragraph: Markierungen\n- paragraph: Vorlücke\n- paragraph: Nachlücke");
        }

        [TestMethod]
        public async Task TrackTableControls_ShouldBeEnabled_WhenSelectingFirstTrackAsync()
        {
            var detailView = new DetailView(TestPage);
            await detailView.GotoAsync();
            await detailView.AddAudiofileAsync();
            await detailView.AddTrackAsync(0);
            await detailView.AddTrackAsync(0);
            await detailView.SelectTracksAsync([1]);
            await Expect(TestPage.GetByLabel("Track table controls")).ToMatchAriaSnapshotAsync(@"- group:
  - button ""Add new track""
  - button ""Edit selected tracks""
  - button
  - button ""Delete all tracks""
  - button
- button ""Fixed table header""");
        }

        [TestMethod]
        public async Task KeyboardCommands_ShouldControlDialogs_WhenUsingEnterOrEscapeAsync()
        {
            var bar = new AppBar(TestPage);
            var detailView = new DetailView(TestPage);
            await detailView.GotoAsync();
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
            await detailView.AddAudiofileAsync();
            await detailView.SetAudiofileInputFileAsync(0, "Kalimba.mp3");
            await detailView.OpenRenameAudiofileDialogAsync(0);
            await Expect(TestPage.GetByRole(AriaRole.Dialog)).ToBeVisibleAsync();
            await detailView.NewFileNameInput.FillAsync("Test 123");
            await TestPage.Keyboard.PressAsync("Enter");
            await TestPage.GetByRole(AriaRole.Dialog).WaitForAsync(new() { State = WaitForSelectorState.Detached });
            await Expect(TestPage.GetByRole(AriaRole.Textbox, new() { Name = "Audiofile" })).ToMatchAriaSnapshotAsync("- textbox \"Audiofile\": Test 123.mp3");
        }
    }
}
