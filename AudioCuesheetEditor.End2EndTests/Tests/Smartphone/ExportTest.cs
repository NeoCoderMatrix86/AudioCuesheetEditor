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
    public class ExportTest : PlaywrightTestBase
    {
        protected override string? DeviceName => "iPhone 13";

        [TestMethod]
        public async Task DownloadCuesheet_GeneratesCuesheetFile_WhenCuesheetIsValid()
        {
            var bar = new AppBar(TestPage);
            var detailView = new DetailView(TestPage);
            await detailView.GotoAsync();
            await detailView.CuesheetArtistInput.FillAsync("Cuesheet Artist 1");
            await detailView.CuesheetTitleInput.FillAsync("Cuesheet Title 1");
            await detailView.AddAudiofileAsync();
            await detailView.SetAudiofileInputFileAsync(0, "Kalimba.mp3");
            await detailView.AddTrackAsync(0);
            await detailView.EditTrackAsync("Track Artist 1", "Track Title 1");
            await bar.OpenExportDialogAsync("Cuesheet");
            var downloadTask = TestPage.WaitForDownloadAsync();
            await TestPage.GetByRole(AriaRole.Button, new() { Name = "Download-Cuesheet.cue" }).ClickAsync();
            var download = await downloadTask;
            using var stream = await download.CreateReadStreamAsync();
            using var reader = new StreamReader(stream);
            var content = await reader.ReadToEndAsync(TestContext.CancellationToken);
            content = content.Replace("\n", Environment.NewLine);
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
            var detailView = new DetailView(TestPage);
            await detailView.GotoAsync();
            await detailView.CuesheetArtistInput.FillAsync("Cuesheet Artist 1");
            await detailView.CuesheetTitleInput.FillAsync("Cuesheet Title 1");
            await detailView.AddAudiofileAsync();
            await detailView.SetAudiofileInputFileAsync(0, "Kalimba.mp3");
            await detailView.AddTrackAsync(0);
            await detailView.EditTrackAsync("Track Artist 1", "Track Title 1");
            await bar.OpenExportDialogAsync("Projectfile");
            var downloadTask = TestPage.WaitForDownloadAsync();
            await TestPage.GetByRole(AriaRole.Button, new() { Name = "Download project" }).ClickAsync();
            var download = await downloadTask;
            using var stream = await download.CreateReadStreamAsync();
            using var reader = new StreamReader(stream);
            var content = await reader.ReadToEndAsync(TestContext.CancellationToken);
            Assert.AreEqual("{\"Artist\":\"Cuesheet Artist 1\",\"Title\":\"Cuesheet Title 1\",\"Audiofiles\":[{\"Name\":\"Kalimba.mp3\",\"Duration\":\"00:05:48.0608330\",\"AudioCodec\":{\"MimeType\":\"audio/mpeg\",\"FileExtension\":\".mp3\",\"Name\":\"AudioCodec MP3\"},\"Tracks\":[{\"Position\":1,\"Artist\":\"Track Artist 1\",\"Title\":\"Track Title 1\",\"Begin\":\"00:00:00\",\"End\":\"00:05:48.0608330\",\"Flags\":[],\"IsLinkedToPreviousTrack\":true}]}]}", content);
        }

        [TestMethod]
        public async Task DownloadText_GeneratesTextFile_WhenCuesheetIsValidAsync()
        {
            var bar = new AppBar(TestPage);
            var detailView = new DetailView(TestPage);
            await detailView.GotoAsync();
            await detailView.CuesheetArtistInput.FillAsync("Cuesheet Artist 1");
            await detailView.CuesheetTitleInput.FillAsync("Cuesheet Title 1");
            await detailView.AddAudiofileAsync();
            await detailView.SetAudiofileInputFileAsync(0, "Kalimba.mp3");
            await detailView.AddTrackAsync(0);
            await detailView.AddTrackAsync(0);
            await detailView.AddTrackAsync(0);
            await detailView.AddTrackAsync(0);
            await detailView.AddTrackAsync(0);
            await detailView.AddTrackAsync(0);
            await detailView.AddTrackAsync(0);
            await detailView.AddTrackAsync(0);
            await detailView.AddTrackAsync(0);
            await detailView.AddTrackAsync(0);
            await detailView.AddTrackAsync(0);
            await detailView.AddTrackAsync(0);
            await detailView.EditTrackAsync("Track Artist 1", "Track Title 1", TimeSpan.Zero, new TimeSpan(0, 0, 30), 1);
            await detailView.EditTrackAsync("Track Artist 2", "Track Title 2", new TimeSpan(0, 0, 30), new TimeSpan(0, 1, 0), 2);
            await detailView.EditTrackAsync("Track Artist 3", "Track Title 3", new TimeSpan(0, 1, 0), new TimeSpan(0, 1, 30), 3);
            await detailView.EditTrackAsync("Track Artist 4", "Track Title 4", new TimeSpan(0, 1, 30), new TimeSpan(0, 2, 0), 4);
            await detailView.EditTrackAsync("Track Artist 5", "Track Title 5", new TimeSpan(0, 2, 0), new TimeSpan(0, 2, 30), 5);
            await detailView.EditTrackAsync("Track Artist 6", "Track Title 6", new TimeSpan(0, 2, 30), new TimeSpan(0, 3, 0), 6);
            await detailView.EditTrackAsync("Track Artist 7", "Track Title 7", new TimeSpan(0, 3, 0), new TimeSpan(0, 3, 30), 7);
            await detailView.EditTrackAsync("Track Artist 8", "Track Title 8", new TimeSpan(0, 3, 30), new TimeSpan(0, 4, 0), 8);
            await detailView.EditTrackAsync("Track Artist 9", "Track Title 9", new TimeSpan(0, 4, 0), new TimeSpan(0, 4, 30), 9);
            await detailView.EditTrackAsync("Track Artist 10", "Track Title 10", new TimeSpan(0, 4, 30), new TimeSpan(0, 5, 0), 10);
            await detailView.EditTrackAsync("Track Artist 11", "Track Title 11", new TimeSpan(0, 5, 0), new TimeSpan(0, 5, 30), 11);
            await detailView.EditTrackAsync("Track Artist 12", "Track Title 12", new TimeSpan(0, 5, 30), null, 12);
            await bar.OpenExportDialogAsync("Textfile");
            await TestPage.GetByRole(AriaRole.Button, new() { Name = "Next", Exact = true }).ClickAsync();
            //TODO: Assert scrollbars
            var downloadTask = TestPage.WaitForDownloadAsync();
            await TestPage.GetByRole(AriaRole.Button, new() { Name = "Download" }).ClickAsync();
            var download = await downloadTask;
            using var stream = await download.CreateReadStreamAsync();
            using var reader = new StreamReader(stream);
            var content = await reader.ReadToEndAsync(TestContext.CancellationToken);
            content = content.Replace("\n", Environment.NewLine);
            //TODO
            Assert.AreEqual(@"Cuesheet Artist 1 - Cuesheet Title 1

Track Artist 1 - Track Title 1 00:00:00

", content);
        }
    }
}
