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
using AudioCuesheetEditor.Model.IO.Import;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AudioCuesheetEditorTests.Model.IO.Import
{
    [TestClass()]
    public class TextImportSchemeTests
    {
        [TestMethod()]
        public void TextImportSchemeValidationTest()
        {
            var importScheme = new TextImportScheme
            {
                SchemeTracks = String.Empty,
                SchemeCuesheet = String.Empty
            };
            Assert.IsFalse(importScheme.IsValid);
            importScheme.SchemeCuesheet = "%Track.Begin% - %Cuesheet.Artist%";
            var validationErrors = importScheme.GetValidationErrorsFiltered(String.Format("{0}.{1}", nameof(TextImportScheme), nameof(TextImportScheme.SchemeCuesheet)));
            Assert.IsTrue(validationErrors.Count == 1);
            Assert.IsTrue(validationErrors.First().Message.Parameter.First().ToString() == "%Track.Begin%");
            importScheme.SchemeCuesheet = "%Track.Begin% - %Cuesheet.Artist% - %Track.Artist%";
            validationErrors = importScheme.GetValidationErrorsFiltered(String.Format("{0}.{1}", nameof(TextImportScheme), nameof(TextImportScheme.SchemeCuesheet)));
            Assert.IsTrue(validationErrors.Count == 1);
            Assert.AreEqual("%Track.Artist%,%Track.Begin%", validationErrors.First().Message.Parameter.First().ToString());
            Boolean eventFiredCorrect = false;
            importScheme.SchemeChanged += delegate (object sender, String property) 
            { 
                if (property == nameof(TextImportScheme.SchemeTracks))
                {
                    eventFiredCorrect = true;
                }
            };
            importScheme.SchemeTracks = "%Cuesheet.Title% - %Track.End%";
            Assert.AreEqual(true, eventFiredCorrect);
            validationErrors = importScheme.GetValidationErrorsFiltered(String.Format("{0}.{1}", nameof(TextImportScheme), nameof(TextImportScheme.SchemeTracks)));
            Assert.IsTrue(validationErrors.Count == 1);
            Assert.AreEqual("%Cuesheet.Title%", validationErrors.First().Message.Parameter.First().ToString());
            importScheme.SchemeCuesheet = "%Cuesheet.Artist% - %Cuesheet.AudioFile%";
            importScheme.SchemeTracks = "%Track.Artist% - %Track.Begin%";
            Assert.IsTrue(importScheme.IsValid);
        }
    }
}
