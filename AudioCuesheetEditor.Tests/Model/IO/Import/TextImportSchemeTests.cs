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
        [TestMethod]
        public void Validate_SchemeCuesheet_WithValidPlaceholders_ShouldReturnSuccess()
        {
            // Arrange
            var scheme = new TextImportScheme
            {
                SchemeCuesheet = "(?'Artist'.+) - (?'Title'.+)\\t+(?'Audiofile'.+)"
            };

            // Act
            var result = scheme.Validate(nameof(TextImportScheme.SchemeCuesheet));

            // Assert
            Assert.AreEqual(ValidationStatus.Success, result.Status);
            Assert.AreEqual(0, result.ValidationMessages.Count);
        }

        [TestMethod]
        public void Validate_SchemeCuesheet_WithSimplePlaceholders_ShouldReturnSuccess()
        {
            // Arrange
            var scheme = new TextImportScheme
            {
                SchemeCuesheet = "Artist - Title"
            };

            // Act
            var result = scheme.Validate(nameof(TextImportScheme.SchemeCuesheet));

            // Assert
            Assert.AreEqual(ValidationStatus.Success, result.Status);
            Assert.AreEqual(0, result.ValidationMessages.Count);
        }

        [TestMethod]
        public void Validate_SchemeCuesheet_WithoutPlaceholders_ShouldReturnError()
        {
            // Arrange
            var scheme = new TextImportScheme
            {
                SchemeCuesheet = "InvalidPattern"
            };

            // Act
            var result = scheme.Validate(nameof(TextImportScheme.SchemeCuesheet));

            // Assert
            Assert.AreEqual(ValidationStatus.Error, result.Status);
            Assert.AreEqual(1, result.ValidationMessages.Count);
            var message = result.ValidationMessages.Single();
            Assert.AreEqual("{0} contains no placeholder!", message.Message);
            Assert.AreEqual(nameof(TextImportScheme.SchemeCuesheet), message.Parameter?.FirstOrDefault());
        }

        [TestMethod]
        public void Validate_SchemeTracks_WithValidPlaceholders_ShouldReturnSuccess()
        {
            // Arrange
            var scheme = new TextImportScheme
            {
                SchemeTracks = "(?'Artist'.+) - (?'Title'.+)(?:...\\t)(?'End'.+)"
            };

            // Act
            var result = scheme.Validate(nameof(TextImportScheme.SchemeTracks));

            // Assert
            Assert.AreEqual(ValidationStatus.Success, result.Status);
            Assert.AreEqual(0, result.ValidationMessages.Count);
        }

        [TestMethod]
        public void Validate_SchemeTracks_WithSimplePlaceholders_ShouldReturnSuccess()
        {
            // Arrange
            var scheme = new TextImportScheme
            {
                SchemeTracks = "Artist - Title\tEnd"
            };

            // Act
            var result = scheme.Validate(nameof(TextImportScheme.SchemeTracks));

            // Assert
            Assert.AreEqual(ValidationStatus.Success, result.Status);
            Assert.AreEqual(0, result.ValidationMessages.Count);
        }

        [TestMethod]
        public void Validate_SchemeTracks_WithoutPlaceholders_ShouldReturnError()
        {
            // Arrange
            var scheme = new TextImportScheme
            {
                SchemeTracks = "InvalidPattern"
            };

            // Act
            var result = scheme.Validate(nameof(TextImportScheme.SchemeTracks));

            // Assert
            Assert.AreEqual(ValidationStatus.Error, result.Status);
            Assert.AreEqual(1, result.ValidationMessages.Count);
            var message = result.ValidationMessages.Single();
            Assert.AreEqual("{0} contains no placeholder!", message.Message);
            Assert.AreEqual(nameof(TextImportScheme.SchemeTracks), message.Parameter?.FirstOrDefault());
        }

        [TestMethod]
        public void Validate_SchemeCuesheetEmpty_ShouldReturnSuccess()
        {
            // Arrange
            var scheme = new TextImportScheme
            {
                SchemeCuesheet = string.Empty
            };

            // Act
            var result = scheme.Validate(nameof(TextImportScheme.SchemeCuesheet));

            // Assert
            Assert.AreEqual(ValidationStatus.Success, result.Status);
            Assert.AreEqual(0, result.ValidationMessages.Count);
        }

        [TestMethod]
        public void Validate_SchemeTrackstEmpty_ShouldReturnSuccess()
        {
            // Arrange
            var scheme = new TextImportScheme
            {
                SchemeTracks = string.Empty
            };

            // Act
            var result = scheme.Validate(nameof(TextImportScheme.SchemeTracks));

            // Assert
            Assert.AreEqual(ValidationStatus.Success, result.Status);
            Assert.AreEqual(0, result.ValidationMessages.Count);
        }
    }
}
