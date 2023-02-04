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
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AudioCuesheetEditorTests.Utility;
using AudioCuesheetEditor.Model.AudioCuesheet;
using System.IO;
using AudioCuesheetEditor.Model.IO.Audio;
using AudioCuesheetEditor.Model.Entity;

namespace AudioCuesheetEditor.Model.IO.Export.Tests
{
    [TestClass()]
    public class ExportProfileTests
    {
        [TestMethod()]
        public void IsExportableTest()
        {
            var exportProfile = new Exportprofile();
            exportProfile.SchemeHead.Scheme = "%Cuesheet.Artist%;%Cuesheet.Title%;%Cuesheet.Cataloguenumber%;%Cuesheet.CDTextfile%";
            Assert.AreEqual(ValidationStatus.Success, exportProfile.SchemeHead.Validate().Status);
            exportProfile.SchemeTracks.Scheme = "%Track.Position%;%Track.Artist%;%Track.Title%;%Track.Begin%;%Track.End%;%Track.Length%;%Track.PreGap%;%Track.PostGap%";
            Assert.AreEqual(ValidationStatus.Success, exportProfile.SchemeTracks.Validate().Status);
            exportProfile.SchemeFooter.Scheme = "Exported %Cuesheet.Title% from %Cuesheet.Artist% using AudioCuesheetEditor at %Date%";
            Assert.AreEqual(ValidationStatus.Success, exportProfile.SchemeFooter.Validate().Status);
            Assert.IsTrue(exportProfile.CanWrite);
            exportProfile.SchemeFooter.Scheme = "Exported %Track.Title% from %Cuesheet.Artist% using AudioCuesheetEditor at %Date%";
            Assert.AreEqual(ValidationStatus.Error, exportProfile.SchemeFooter.Validate().Status);
            Assert.IsFalse(exportProfile.CanWrite);
        }
    }
}