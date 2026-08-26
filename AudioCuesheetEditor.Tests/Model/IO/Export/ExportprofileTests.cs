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
using System.Linq;

namespace AudioCuesheetEditor.Tests.Model.IO.Export
{
    [TestClass()]
    public class ExportprofileTests
    {
        [TestMethod()]
        public void Validate_EmptyFilename_ReturnsError()
        {
            // Arrange
            var exportprofile = new Exportprofile
            {
                Filename = string.Empty
            };
            // Act
            var validationResult = exportprofile.Validate(nameof(Exportprofile.Filename));
            // Assert
            Assert.AreEqual(ValidationStatus.Error, validationResult.Status);
            Assert.AreEqual("{0} has no value!", validationResult.ValidationMessages.First().Message);
            Assert.AreEqual(nameof(Exportprofile.Filename), validationResult.ValidationMessages.First().Parameter?.First().ToString());
        }

        [TestMethod()]
        public void Validate_EmptyName_ReturnsError()
        {
            // Arrange
            var exportprofile = new Exportprofile
            {
                Name = string.Empty
            };
            // Act
            var validationResult = exportprofile.Validate(nameof(Exportprofile.Name));
            // Assert
            Assert.AreEqual(ValidationStatus.Error, validationResult.Status);
            Assert.AreEqual("{0} has no value!", validationResult.ValidationMessages.First().Message);
            Assert.AreEqual(nameof(Exportprofile.Name), validationResult.ValidationMessages.First().Parameter?.First().ToString());
        }

        [TestMethod()]
        public void Validate_SchemeHeadWithTrackPlaceholder_ReturnsError()
        {
            // Arrange
            var exportprofile = new Exportprofile
            {
                SchemeHead = Exportprofile.SchemeTrackTitle
            };
            // Act
            var validationResult = exportprofile.Validate(nameof(Exportprofile.SchemeHead));
            // Assert
            Assert.AreEqual(ValidationStatus.Error, validationResult.Status);
            Assert.AreEqual("{0} contains placeholder '{1}' that can not be resolved!", validationResult.ValidationMessages.First().Message);
            Assert.AreEqual(nameof(Exportprofile.SchemeHead), validationResult.ValidationMessages.First().Parameter?.First().ToString());
            Assert.AreEqual(Exportprofile.SchemeTrackTitle, validationResult.ValidationMessages.First().Parameter?.ElementAt(1).ToString());
        }

        [TestMethod()]
        public void Validate_SchemeTrackWithCuesheetPlaceholder_ReturnsError()
        {
            // Arrange
            var exportprofile = new Exportprofile
            {
                SchemeTracks = Exportprofile.SchemeCuesheetArtist
            };
            // Act
            var validationResult = exportprofile.Validate(nameof(Exportprofile.SchemeTracks));
            // Assert
            Assert.AreEqual(ValidationStatus.Error, validationResult.Status);
            Assert.AreEqual("{0} contains placeholder '{1}' that can not be resolved!", validationResult.ValidationMessages.First().Message);
            Assert.AreEqual(nameof(Exportprofile.SchemeTracks), validationResult.ValidationMessages.First().Parameter?.First().ToString());
            Assert.AreEqual(Exportprofile.SchemeCuesheetArtist, validationResult.ValidationMessages.First().Parameter?.ElementAt(1).ToString());
        }

        [TestMethod()]
        public void Validate_SchemeFooterWithTrackPlaceholder_ReturnsError()
        {
            // Arrange
            var exportprofile = new Exportprofile
            {
                SchemeFooter = Exportprofile.SchemeTrackBegin
            };
            // Act
            var validationResult = exportprofile.Validate(nameof(Exportprofile.SchemeFooter));
            // Assert
            Assert.AreEqual(ValidationStatus.Error, validationResult.Status);
            Assert.AreEqual("{0} contains placeholder '{1}' that can not be resolved!", validationResult.ValidationMessages.First().Message);
            Assert.AreEqual(nameof(Exportprofile.SchemeFooter), validationResult.ValidationMessages.First().Parameter?.First().ToString());
            Assert.AreEqual(Exportprofile.SchemeTrackBegin, validationResult.ValidationMessages.First().Parameter?.ElementAt(1).ToString());
        }

        [TestMethod()]
        public void Validate_SchemeHeadCorrectPlaceholder_ReturnsSuccess()
        {
            // Arrange
            var exportprofile = new Exportprofile
            {
                SchemeHead = Exportprofile.SchemeCuesheetTitle
            };
            // Act
            var validationResult = exportprofile.Validate(nameof(Exportprofile.SchemeHead));
            // Assert
            Assert.AreEqual(ValidationStatus.Success, validationResult.Status);
            Assert.IsEmpty(validationResult.ValidationMessages);
        }

        [TestMethod()]
        public void Validate_SchemeTracksCorrectPlaceholder_ReturnsSuccess()
        {
            // Arrange
            var exportprofile = new Exportprofile
            {
                SchemeTracks = Exportprofile.SchemeTrackPreGap
            };
            // Act
            var validationResult = exportprofile.Validate(nameof(Exportprofile.SchemeTracks));
            // Assert
            Assert.AreEqual(ValidationStatus.Success, validationResult.Status);
            Assert.IsEmpty(validationResult.ValidationMessages);
        }

        [TestMethod()]
        public void Validate_SchemeAudiofilesWithTrackPlaceholder_ReturnsError()
        {
            // Arrange
            var exportprofile = new Exportprofile
            {
                SchemeAudiofiles = Exportprofile.SchemeTrackArtist
            };
            // Act
            var validationResult = exportprofile.Validate(nameof(Exportprofile.SchemeAudiofiles));
            // Assert
            Assert.AreEqual(ValidationStatus.Error, validationResult.Status);
            Assert.AreEqual("{0} contains placeholder '{1}' that can not be resolved!", validationResult.ValidationMessages.First().Message);
            Assert.AreEqual(nameof(Exportprofile.SchemeAudiofiles), validationResult.ValidationMessages.First().Parameter?.First().ToString());
            Assert.AreEqual(Exportprofile.SchemeTrackArtist, validationResult.ValidationMessages.First().Parameter?.ElementAt(1).ToString());
        }

        [TestMethod()]
        public void Validate_SchemeAudiofilesCorrectPlaceholder_ReturnsNoValidation()
        {
            // Arrange
            var exportprofile = new Exportprofile
            {
                SchemeAudiofiles = Exportprofile.SchemeAudiofileName
            };
            // Act
            var validationResult = exportprofile.Validate(nameof(Exportprofile.SchemeAudiofiles));
            // Assert
            Assert.AreEqual(ValidationStatus.Success, validationResult.Status);
            Assert.IsEmpty(validationResult.ValidationMessages);
        }
    }
}