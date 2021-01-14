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
using System.IO;
using AudioCuesheetEditor.Model.IO;

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
            var testHelper = new TestHelper();
            var cuesheet = testHelper.CuesheetController.Cuesheet;
            var track1 = testHelper.CuesheetController.NewTrack();
            cuesheet.AddTrack(track1);
            var track2 = testHelper.CuesheetController.NewTrack();
            cuesheet.AddTrack(track2);
            var track3 = testHelper.CuesheetController.NewTrack();
            cuesheet.AddTrack(track3);
            Assert.AreEqual(cuesheet.Tracks.Count, 3);
            Assert.IsTrue(track1.Position.Value == 1);
            cuesheet.MoveTrack(track1, MoveDirection.Up);
            Assert.IsTrue(track1.Position.Value == 1);
            Assert.IsTrue(track3.Position.Value == 3);
            cuesheet.MoveTrack(track3, MoveDirection.Down);
            Assert.IsTrue(track3.Position.Value == 3);
            Assert.IsTrue(track2.Position.Value == 2);
            cuesheet.MoveTrack(track2, MoveDirection.Up);
            Assert.IsTrue(track2.Position.Value == 1);
            Assert.IsTrue(track1.Position.Value == 2);
            cuesheet.MoveTrack(track2, MoveDirection.Down);
            cuesheet.MoveTrack(track2, MoveDirection.Down);
            Assert.IsTrue(track2.Position.Value == 3);
            Assert.IsTrue(track1.Position.Value == 1);
            Assert.IsTrue(track3.Position.Value == 2);
        }

        [TestMethod()]
        public void ImportTest()
        {
            //Prepare text input file
            StringBuilder builder = new StringBuilder();
            builder.AppendLine("Sample Artist 1 - Sample Title 1				00:05:00");
            builder.AppendLine("Sample Artist 2 - Sample Title 2				00:09:23");
            builder.AppendLine("Sample Artist 3 - Sample Title 3				00:15:54");
            builder.AppendLine("Sample Artist 4 - Sample Title 4				00:20:13");
            builder.AppendLine("Sample Artist 5 - Sample Title 5				00:24:54");
            builder.AppendLine("Sample Artist 6 - Sample Title 6				00:31:54");
            builder.AppendLine("Sample Artist 7 - Sample Title 7				00:45:54");
            builder.AppendLine("Sample Artist 8 - Sample Title 8				01:15:54");

            var tempFile = Path.GetTempFileName();
            File.WriteAllText(tempFile, builder.ToString());

            //Test TextImportFile
            var textImportFile = new TextImportFile(new MemoryStream(File.ReadAllBytes(tempFile)))
            {
                ImportScheme = "%Artist% - %Title%[\t]{1,}%End%"
            };
            Assert.IsNull(textImportFile.AnalyseException);
            Assert.IsTrue(textImportFile.Tracks.Count == 8);
            Assert.IsTrue(textImportFile.IsValid);

            var testHelper = new TestHelper();
            var cuesheet = testHelper.CuesheetController.Cuesheet;
            cuesheet.Import(textImportFile);

            Assert.IsNull(cuesheet.CDTextfile);
            Assert.AreEqual(cuesheet.ValidationErrors.Count, 5);

            File.Delete(tempFile);
        }
    }
}