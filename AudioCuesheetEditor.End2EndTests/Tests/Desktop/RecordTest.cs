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

namespace AudioCuesheetEditor.End2EndTests.Tests.Desktop
{
    [TestClass]
    public class RecordTest : PlaywrightTestBase
    {
        [TestMethod]
        public async Task Record_ShouldRecordTracks_WhenTracksAdded()
        {
            var recordView = new RecordView(TestPage);
            var viewModes = new ViewModes(TestPage, DeviceName != null);
            var detailView = new DetailView(TestPage, DeviceName != null);
            await recordView.GotoAsync();
            await recordView.StartRecordingAsync();
            await recordView.AddRecordingTrackAsync("Test Track 1 Artist", "Test Track 1 Title");
            await recordView.AddRecordingTrackAsync("Test Track 2 Artist", "Test Track 2 Title");
            await recordView.StopRecordingAsync();
            await Expect(TestPage.GetByRole(AriaRole.Cell, new() { Name = "Test Track 1 Artist Clear" })).ToBeVisibleAsync();
            await Expect(TestPage.GetByRole(AriaRole.Cell, new() { Name = "Test Track 1 Title Clear" })).ToBeVisibleAsync();
            await Expect(TestPage.GetByRole(AriaRole.Cell, new() { Name = "Test Track 2 Artist Clear" })).ToBeVisibleAsync();
            await Expect(TestPage.GetByRole(AriaRole.Cell, new() { Name = "Test Track 2 Title Clear" })).ToBeVisibleAsync();
            await viewModes.SwitchView("Detail view");
            await detailView.SelectTracksAsync([1]);
            await detailView.EditSelectedTracksModalAsync();
            await Expect(TestPage.GetByRole(AriaRole.Checkbox, new() { Name = "Link to previous track" })).ToBeCheckedAsync();
        }
    }
}
