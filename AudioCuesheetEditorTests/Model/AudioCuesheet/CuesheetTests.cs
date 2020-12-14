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
using Microsoft.VisualStudio.TestTools.UnitTesting;
using AudioCuesheetEditor.Model.AudioCuesheet;
using System;
using System.Collections.Generic;
using System.Text;
using AudioCuesheetEditorTests.Utility;
using System.Linq;

namespace AudioCuesheetEditor.Model.AudioCuesheet.Tests
{
    [TestClass()]
    public class CuesheetTests
    {
        [TestMethod()]
        public void AddTrackTest()
        {
            var testHelper = new TestHelper();
            var cuesheet = testHelper.CuesheetController.Cuesheet;
            Assert.AreEqual(cuesheet.Tracks.Count, 0);
            cuesheet.AddTrack(new Track(testHelper.CuesheetController));
            Assert.AreEqual(cuesheet.Tracks.Count, 1);
        }

        [TestMethod()]
        public void CuesheetTest()
        {
            var testHelper = new TestHelper();
            var cuesheet = testHelper.CuesheetController.Cuesheet;
            Assert.IsNull(cuesheet.AudioFile);
            var validationErrorAudioFile = cuesheet.GetValidationErrorsFiltered(String.Format("{0}.{1}", nameof(Cuesheet), nameof(Cuesheet.AudioFile))).FirstOrDefault();
            Assert.IsNotNull(validationErrorAudioFile);
            cuesheet.AudioFile = new IO.AudioFile("AudioFile01.ogg");
            validationErrorAudioFile = cuesheet.GetValidationErrorsFiltered(nameof(Cuesheet.AudioFile)).FirstOrDefault();
            Assert.IsNull(validationErrorAudioFile);
        }

        [TestMethod()]
        public void MoveTrackTest()
        {
            //DEBUG
            Assert.IsTrue(false);
            //DEBUG
            var testHelper = new TestHelper();
            var cuesheet = testHelper.CuesheetController.Cuesheet;
            var track1 = testHelper.CuesheetController.NewTrack();
            cuesheet.AddTrack(track1);
            var track2 = testHelper.CuesheetController.NewTrack();
            cuesheet.AddTrack(track2);
            var track3 = testHelper.CuesheetController.NewTrack();
            cuesheet.AddTrack(track3);
            Assert.AreEqual(cuesheet.Tracks.Count, 3);
            Assert.IsTrue(track1.Position == 1);
            cuesheet.MoveTrack(track1, MoveDirection.Up);
            Assert.IsTrue(track1.Position == 1);
            Assert.IsTrue(track3.Position == 3);
            cuesheet.MoveTrack(track3, MoveDirection.Down);
            Assert.IsTrue(track3.Position == 3);
            Assert.IsTrue(track2.Position == 2);
            cuesheet.MoveTrack(track2, MoveDirection.Up);
            Assert.IsTrue(track2.Position == 1);
            Assert.IsTrue(track1.Position == 2);
            cuesheet.MoveTrack(track2, MoveDirection.Down);
            cuesheet.MoveTrack(track2, MoveDirection.Down);
            Assert.IsTrue(track2.Position == 3);
            Assert.IsTrue(track1.Position == 1);
            Assert.IsTrue(track3.Position == 2);
        }
    }
}