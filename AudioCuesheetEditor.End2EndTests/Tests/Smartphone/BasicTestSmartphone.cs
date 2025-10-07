using AudioCuesheetEditor.End2EndTests.Models;
using Microsoft.Playwright;

namespace AudioCuesheetEditor.End2EndTests.Tests.Smartphone
{
    [TestClass]
    public class BasicTestSmartphone : PlaywrightTestBase
    {
        protected override string? DeviceName => "iPhone 13";

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
        public async Task ChangeLanguage_ShouldShowGermanHeadings_WhenGermanIsSelected()
        {
            var bar = new AppBar(TestPage);
            var detailView = new DetailView(TestPage, DeviceName != null);
            await detailView.GotoAsync();
            await bar.ChangeLanguageAsync("German (Germany)");
            await Expect(TestPage.GetByRole(AriaRole.Heading, new() { Name = "Abschnitte" })).ToBeVisibleAsync();
            await Expect(TestPage.GetByRole(AriaRole.Heading, new() { Name = "Allgemeine Informationen" })).ToBeVisibleAsync();
            await Expect(TestPage.GetByText("Aufnahmeansicht")).ToBeVisibleAsync();
            await Expect(TestPage.GetByRole(AriaRole.Heading, new() { Name = "Titel" })).ToBeVisibleAsync();
            await Expect(TestPage.GetByRole(AriaRole.Heading, new() { Name = "Wiedergabe" })).ToBeVisibleAsync();
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
  - button
  - button ""Alle Titel löschen""
  - button
- button ""Fester Tabellenkopf""");
        }
    }
}
