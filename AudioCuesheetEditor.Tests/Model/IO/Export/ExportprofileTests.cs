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
using AudioCuesheetEditor.Model.Entity;
using AudioCuesheetEditor.Model.IO.Export;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;

namespace AudioCuesheetEditor.Tests.Model.IO.Export
{
    [TestClass()]
    public class ExportprofileTests
    {
        [TestMethod()]
        public void ValidateTest()
        {
            var exportprofile = new Exportprofile
            {
                Filename = string.Empty
            };
            Assert.AreEqual(ValidationStatus.Error, exportprofile.Validate(nameof(Exportprofile.Filename)).Status);
            exportprofile.Filename = "Test123";
            Assert.AreEqual(ValidationStatus.Success, exportprofile.Validate(nameof(Exportprofile.Filename)).Status);
            exportprofile.SchemeHead = "%Cuesheet.Artist%;%Cuesheet.Title%;%Cuesheet.Cataloguenumber%;%Cuesheet.CDTextfile%";
            Assert.AreEqual(ValidationStatus.Success, exportprofile.Validate(nameof(Exportprofile.SchemeHead)).Status);
            exportprofile.SchemeTracks = "%Track.Position%;%Track.Artist%;%Track.Title%;%Track.Begin%;%Track.End%;%Track.Length%;%Track.PreGap%;%Track.PostGap%";
            Assert.AreEqual(ValidationStatus.Success, exportprofile.Validate(nameof(Exportprofile.SchemeTracks)).Status);
            exportprofile.SchemeFooter = "Exported %Cuesheet.Title% from %Cuesheet.Artist% using AudioCuesheetEditor at %Date%";
            Assert.AreEqual(ValidationStatus.Success, exportprofile.Validate(nameof(Exportprofile.SchemeFooter)).Status);
            exportprofile.SchemeFooter = "Exported %Track.Title% from %Cuesheet.Artist% using AudioCuesheetEditor at %Date%";
            Assert.AreEqual(ValidationStatus.Error, exportprofile.Validate(nameof(Exportprofile.SchemeFooter)).Status);
        }
    }
}