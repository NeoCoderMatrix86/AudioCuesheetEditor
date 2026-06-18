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
using AudioCuesheetEditor.Model.AudioCuesheet;
using AudioCuesheetEditor.Model.Entity;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Linq;

namespace AudioCuesheetEditor.Tests.Model.AudioCuesheet
{
    [TestClass()]
    public class CuesheetTests
    {
        [TestMethod]
        public void Validate_AudiofilesEmpty_ReturnsValidationStatusError()
        {
            // Arrange
            var cuesheet = new Cuesheet();
            // Act
            var validationResult = cuesheet.Validate(nameof(Cuesheet.Audiofiles));
            // Assert
            Assert.AreEqual(ValidationStatus.Error, validationResult.Status);
        }

        [TestMethod]
        public void Validate_ArtistNull_ReturnsValidationStatusError()
        {
            // Arrange
            var cuesheet = new Cuesheet();
            // Act
            var validationResult = cuesheet.Validate(nameof(Cuesheet.Artist));
            // Assert
            Assert.AreEqual(ValidationStatus.Error, validationResult.Status);
        }

        [TestMethod]
        public void Validate_TitleNull_ReturnsValidationStatusError()
        {
            // Arrange
            var cuesheet = new Cuesheet();
            // Act
            var validationResult = cuesheet.Validate(nameof(Cuesheet.Title));
            // Assert
            Assert.AreEqual(ValidationStatus.Error, validationResult.Status);
        }

        [TestMethod]
        public void Validate_CataloguenumberWithLetters_ReturnsValidationStatusError()
        {
            // Arrange
            var cuesheet = new Cuesheet()
            {
                Cataloguenumber = nameof(Validate_CataloguenumberWithLetters_ReturnsValidationStatusError)
            };
            // Act
            var validationResult = cuesheet.Validate(nameof(Cuesheet.Cataloguenumber));
            // Assert
            Assert.AreEqual(ValidationStatus.Error, validationResult.Status);
            Assert.AreEqual("{0} must only contain numbers!", validationResult.ValidationMessages.First().Message);
            Assert.AreEqual(nameof(Cuesheet.Cataloguenumber), validationResult.ValidationMessages.First().Parameter?.First().ToString());
        }

        [TestMethod]
        public void Validate_CataloguenumberLengthUnequal13_ReturnsValidationStatusError()
        {
            // Arrange
            var cuesheet = new Cuesheet()
            {
                Cataloguenumber = "012345678"
            };
            // Act
            var validationResult = cuesheet.Validate(nameof(Cuesheet.Cataloguenumber));
            // Assert
            Assert.AreEqual(ValidationStatus.Error, validationResult.Status);
            Assert.AreEqual("{0} has an invalid length. Allowed length is {1}!", validationResult.ValidationMessages.First().Message);
            Assert.AreEqual(nameof(Cuesheet.Cataloguenumber), validationResult.ValidationMessages.First().Parameter?.First().ToString());
            Assert.AreEqual(13, validationResult.ValidationMessages.First().Parameter?.Last());
        }
        
        //TODO: Validation for audio files
    }
}