using AudioCuesheetEditor.End2EndTests.Models;
using Microsoft.Playwright;

namespace AudioCuesheetEditor.End2EndTests.Tests.Desktop
{
    [TestClass]
    public class ExportTest : PlaywrightTestBase
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
            await bar.ExportCuesheetAsync();
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

        //TODO: Test export project
        //TODO: Test export textfile
    }
}
