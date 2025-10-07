using AudioCuesheetEditor.End2EndTests.Models;
using Microsoft.Playwright;

namespace AudioCuesheetEditor.End2EndTests.Tests.Smartphone
{
    [TestClass]
    public class ExportTestSmartphone : PlaywrightTestBase
    {
        [TestMethod]
        public async Task DownloadCuesheet_GeneratesCuesheetFile_WhenCuesheetIsValid()
        {
            var bar = new AppBar(TestPage);
            var detailView = new DetailView(TestPage, DeviceName != null);
            await detailView.GotoAsync();
            await detailView.AddTrackAsync();
            await detailView.CuesheetArtistInput.FillAsync("Cuesheet Artist 1");
            await detailView.CuesheetTitleInput.FillAsync("Cuesheet Title 1");
            await detailView.AudiofileInput.SetInputFilesAsync("Kalimba.mp3");
            await detailView.EditTrackAsync("Track Artist 1", "Track Title 1");
            await bar.OpenExportDialogAsync("Cuesheet");
            var downloadTask = TestPage.WaitForDownloadAsync();
            await TestPage.GetByRole(AriaRole.Button, new() { Name = "Download-Cuesheet.cue" }).ClickAsync();
            var download = await downloadTask;
            using var stream = await download.CreateReadStreamAsync();
            using var reader = new StreamReader(stream);
            var content = await reader.ReadToEndAsync(TestContext.CancellationTokenSource.Token);
            content = content.Replace("\n", "\r\n");
            Assert.AreEqual(@"TITLE ""Cuesheet Title 1""
PERFORMER ""Cuesheet Artist 1""
FILE ""Kalimba.mp3"" MP3
	TRACK 01 AUDIO
		TITLE ""Track Title 1""
		PERFORMER ""Track Artist 1""
		INDEX 01 00:00:00
", content);
        }

        [TestMethod]
        public async Task DownloadProject_GeneratesProjectFile_WhenCuesheetIsValidAsync()
        {
            var bar = new AppBar(TestPage);
            var detailView = new DetailView(TestPage, DeviceName != null);
            await detailView.GotoAsync();
            await detailView.AddTrackAsync();
            await detailView.CuesheetArtistInput.FillAsync("Cuesheet Artist 1");
            await detailView.CuesheetTitleInput.FillAsync("Cuesheet Title 1");
            await detailView.AudiofileInput.SetInputFilesAsync("Kalimba.mp3");
            await detailView.EditTrackAsync("Track Artist 1", "Track Title 1");
            await bar.OpenExportDialogAsync("Projectfile");
            var downloadTask = TestPage.WaitForDownloadAsync();
            await TestPage.GetByRole(AriaRole.Button, new() { Name = "Download project" }).ClickAsync();
            var download = await downloadTask;
            using var stream = await download.CreateReadStreamAsync();
            using var reader = new StreamReader(stream);
            var content = await reader.ReadToEndAsync(TestContext.CancellationTokenSource.Token);
            Assert.AreEqual("{\"Tracks\":[{\"Position\":1,\"Artist\":\"Track Artist 1\",\"Title\":\"Track Title 1\",\"Begin\":\"00:00:00\",\"End\":\"00:05:48\",\"Flags\":[],\"IsLinkedToPreviousTrack\":true}],\"Artist\":\"Cuesheet Artist 1\",\"Title\":\"Cuesheet Title 1\",\"Audiofile\":{\"Name\":\"Kalimba.mp3\",\"Duration\":\"00:05:48\",\"AudioCodec\":{\"MimeType\":\"audio/mpeg\",\"FileExtension\":\".mp3\",\"Name\":\"AudioCodec MP3\"}},\"Sections\":[]}", content);
        }

        [TestMethod]
        public async Task DownloadText_GeneratesTextFile_WhenCuesheetIsValidAsync()
        {
            var bar = new AppBar(TestPage);
            var detailView = new DetailView(TestPage, DeviceName != null);
            await detailView.GotoAsync();
            await detailView.AddTrackAsync();
            await detailView.CuesheetArtistInput.FillAsync("Cuesheet Artist 1");
            await detailView.CuesheetTitleInput.FillAsync("Cuesheet Title 1");
            await detailView.AudiofileInput.SetInputFilesAsync("Kalimba.mp3");
            await detailView.EditTrackAsync("Track Artist 1", "Track Title 1");
            await bar.OpenExportDialogAsync("Textfile");
            await TestPage.GetByRole(AriaRole.Button, new() { Name = "Next" }).ClickAsync();
            var downloadTask = TestPage.WaitForDownloadAsync();
            await TestPage.GetByRole(AriaRole.Button, new() { Name = "Download-YouTube.txt" }).ClickAsync();
            var download = await downloadTask;
            using var stream = await download.CreateReadStreamAsync();
            using var reader = new StreamReader(stream);
            var content = await reader.ReadToEndAsync(TestContext.CancellationTokenSource.Token);
            content = content.Replace("\n", "\r\n");
            Assert.AreEqual(@"Cuesheet Artist 1 - Cuesheet Title 1
Track Artist 1 - Track Title 1 00:00:00

", content);
        }
    }
}
