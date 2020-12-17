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
using AudioCuesheetEditor.Model.IO.Export;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AudioCuesheetEditorTests.Utility;
using AudioCuesheetEditor.Model.AudioCuesheet;
using System.IO;

namespace AudioCuesheetEditor.Model.IO.Export.Tests
{
    [TestClass()]
    public class ExportProfileTests
    {
        [TestMethod()]
        public void ExportProfileTest()
        {
            //Prepare cuesheet
            var testHelper = new TestHelper();
            Cuesheet cuesheet = testHelper.CuesheetController.Cuesheet;
            cuesheet.Artist = "Demo Artist";
            cuesheet.Title = "Demo Title";
            cuesheet.AudioFile = new AudioFile("Testfile.mp3");
            var begin = TimeSpan.Zero;
            for (int i = 1; i < 25; i++)
            {
                var track = testHelper.CuesheetController.NewTrack();
                track.Artist = String.Format("Demo Track Artist {0}", i);
                track.Title = String.Format("Demo Track Title {0}", i);
                track.Begin = begin;
                begin = begin.Add(new TimeSpan(0, i, i));
                track.End = begin;
                cuesheet.AddTrack(track);
            }

            //Test class
            var exportProfile = new ExportProfile(testHelper.Localizer, cuesheet);
            exportProfile.SchemeHead.Scheme = "%Cuesheet.Artist%;%Cuesheet.Title%";
            Assert.IsTrue(exportProfile.SchemeHead.IsValid);
            exportProfile.SchemeTracks.Scheme = "%Track.Position%;%Track.Artist%;%Track.Title%;%Track.Begin%;%Track.End%;%Track.Length%";
            Assert.IsTrue(exportProfile.SchemeTracks.IsValid);
            exportProfile.SchemeFooter.Scheme = "Exported %Cuesheet.Title% from %Cuesheet.Artist% using AudioCuesheetEditor";
            Assert.IsTrue(exportProfile.SchemeFooter.IsValid);
            Assert.IsTrue(exportProfile.IsExportable);
            var fileContent = exportProfile.GenerateExport();
            Assert.IsNotNull(fileContent);
            var tempFile = Path.GetTempFileName();
            File.WriteAllBytes(tempFile, fileContent);
            var content = File.ReadAllLines(tempFile);
            Assert.AreEqual(content[0], "Demo Artist;Demo Title");
            for (int i = 1; i < content.Length - 1;i++)
            {
                Assert.IsFalse(String.IsNullOrEmpty(content[i]));
                Assert.AreNotEqual(content[i], ";;;;;");
                Assert.IsTrue(content[i].StartsWith(cuesheet.Tracks.ToList()[i - 1].Position + ";"));
            }
            Assert.AreEqual(content[^1], "Exported Demo Title from Demo Artist using AudioCuesheetEditor");

            File.Delete(tempFile);

            exportProfile.SchemeHead.Scheme = "%Track.Position%;%Cuesheet.Artist%;";
            Assert.IsFalse(exportProfile.SchemeHead.IsValid);
            Assert.AreEqual(exportProfile.SchemeHead.ValidationErrors.Count, 1);

            //Check multiline export

            exportProfile = new ExportProfile(testHelper.Localizer, cuesheet);
            exportProfile.SchemeHead.Scheme = "%Cuesheet.Artist%;%Cuesheet.Title%";
            Assert.IsTrue(exportProfile.SchemeHead.IsValid);
            exportProfile.SchemeTracks.Scheme = String.Format("%Track.Position%{0}%Track.Artist%{1}%Track.Title%;%Track.Begin%;%Track.End%;%Track.Length%", Environment.NewLine, Environment.NewLine);
            Assert.IsTrue(exportProfile.SchemeTracks.IsValid);
            exportProfile.SchemeFooter.Scheme = "Exported %Cuesheet.Title% from %Cuesheet.Artist% using AudioCuesheetEditor";
            Assert.IsTrue(exportProfile.SchemeFooter.IsValid);
            Assert.IsTrue(exportProfile.IsExportable);
            fileContent = exportProfile.GenerateExport();
            Assert.IsNotNull(fileContent);
            tempFile = Path.GetTempFileName();
            File.WriteAllBytes(tempFile, fileContent);
            content = File.ReadAllLines(tempFile);
            Assert.AreEqual(content[0], "Demo Artist;Demo Title");
            var trackPosition = 0;
            for (int i = 1; i < content.Length - 1; i += 3)
            {
                Assert.IsFalse(String.IsNullOrEmpty(content[i]));
                Assert.AreNotEqual(content[i], ";;;;;");
                Assert.IsTrue(content[i].StartsWith(cuesheet.Tracks.ToList()[trackPosition].Position.Value.ToString()));
                trackPosition++;
            }
            Assert.AreEqual(content[^1], "Exported Demo Title from Demo Artist using AudioCuesheetEditor");

            File.Delete(tempFile);

        }
    }
}