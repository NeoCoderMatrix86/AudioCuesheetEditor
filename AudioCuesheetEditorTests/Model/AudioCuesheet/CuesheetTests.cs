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
            var cuesheet = new Cuesheet(TestHelper.GetCuesheetController());
            Assert.AreEqual(cuesheet.Tracks.Count, 0);
            cuesheet.AddTrack(new Track(TestHelper.GetCuesheetController()));
            Assert.AreEqual(cuesheet.Tracks.Count, 1);
        }

        [TestMethod()]
        public void CuesheetTest()
        {
            var cuesheet = new Cuesheet(TestHelper.GetCuesheetController());
            Assert.IsNull(cuesheet.AudioFile);
            var validationErrorAudioFile = cuesheet.GetValidationErrorsFiltered(String.Format("{0}.{1}", nameof(Cuesheet), nameof(Cuesheet.AudioFile))).FirstOrDefault();
            Assert.IsNotNull(validationErrorAudioFile);
            cuesheet.AudioFile = new IO.AudioFile("AudioFile01.ogg");
            validationErrorAudioFile = cuesheet.GetValidationErrorsFiltered(nameof(Cuesheet.AudioFile)).FirstOrDefault();
            Assert.IsNull(validationErrorAudioFile);
        }
    }
}