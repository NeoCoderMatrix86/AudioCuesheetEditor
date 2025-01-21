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
using AudioCuesheetEditor.Model.IO.Import;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Linq;

namespace AudioCuesheetEditor.Tests.Model.IO.Import
{
    [TestClass()]
    public class TextImportSchemeTests
    {
        [TestMethod()]
        public void TextImportSchemeValidationTest()
        {
            var importScheme = new TextImportScheme
            {
                SchemeTracks = string.Empty,
                SchemeCuesheet = string.Empty
            };
            Assert.AreEqual(ValidationStatus.Success, importScheme.Validate().Status);
            importScheme.SchemeCuesheet = "(?'Track.Begin'\\w{1,}) - (?'Cuesheet.Artist'\\w{1})";
            var validationResult = importScheme.Validate(nameof(TextImportScheme.SchemeCuesheet));
            Assert.AreEqual(ValidationStatus.Error, validationResult.Status);
            Assert.IsTrue(validationResult.ValidationMessages?.Any(x => x.Parameter != null && x.Parameter.Last().Equals("(?'Track.Begin'\\w{1,})")));
            importScheme.SchemeCuesheet = "(?'Track.Begin'\\w{1,}) - (?'Cuesheet.Artist'\\w{1}) - (?'Track.Artist'[A-z0-9]{1,})";
            validationResult = importScheme.Validate(nameof(TextImportScheme.SchemeCuesheet));
            Assert.AreEqual(ValidationStatus.Error, validationResult.Status);
            Assert.IsTrue(validationResult.ValidationMessages?.Count == 2);
            Assert.IsTrue(validationResult.ValidationMessages?.Any(x => x.Parameter != null && x.Parameter.Last().Equals("(?'Track.Begin'\\w{1,})")));
            Assert.IsTrue(validationResult.ValidationMessages?.Any(x => x.Parameter != null && x.Parameter.Last().Equals("(?'Track.Artist'[A-z0-9]{1,})")));
            bool eventFiredCorrect = false;
            importScheme.SchemeChanged += delegate (object? sender, string property)
            {
                if (property == nameof(TextImportScheme.SchemeTracks))
                {
                    eventFiredCorrect = true;
                }
            };
            importScheme.SchemeTracks = "(?'Cuesheet.Title'[a-zA-Z0-9_ .;äöü&:,]{1,}) - (?'Track.End'\\w{1,})";
            Assert.AreEqual(true, eventFiredCorrect);
            validationResult = importScheme.Validate(nameof(TextImportScheme.SchemeTracks));
            Assert.AreEqual(ValidationStatus.Error, validationResult.Status);
            Assert.IsTrue(validationResult.ValidationMessages?.Any(x => x.Parameter != null && x.Parameter.Last().Equals("(?'Cuesheet.Title'[a-zA-Z0-9_ .;äöü&:,]{1,})")));
            importScheme.SchemeCuesheet = "(?'Cuesheet.Artist'\\w{1,}) - (?'Cuesheet.AudioFile'[a-zA-Z0-9_. ;äöü&:,\\]{1,})";
            importScheme.SchemeTracks = "(?'Track.Artist'[A-z0-9]{1,}) - (?'Track.Title'.{1,})";
            Assert.AreEqual(ValidationStatus.Success, importScheme.Validate().Status);
        }
    }
}
