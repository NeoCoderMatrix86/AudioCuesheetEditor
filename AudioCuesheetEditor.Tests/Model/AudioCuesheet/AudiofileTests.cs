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
using AudioCuesheetEditor.Model.IO.Audio;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Linq;

namespace AudioCuesheetEditor.Tests.Model.AudioCuesheet
{
    [TestClass]
    public class AudiofileTests
    {
        [TestMethod]
        public void Validate_FilenameNull_ReturnsValidationStatusError()
        {
            // Arrange
            var audiofile = new Audiofile();
            // Act
            var validationResult = audiofile.Validate(nameof(Audiofile.Name));
            // Assert
            Assert.AreEqual(ValidationStatus.Error, validationResult.Status);
        }

        [TestMethod]
        public void Validate_TracksEmpty_ReturnsValidationStatusError()
        {
            // Arrange
            var audiofile = new Audiofile();
            // Act
            var validationResult = audiofile.Validate(nameof(Audiofile.Tracks));
            // Assert
            Assert.AreEqual(ValidationStatus.Error, validationResult.Status);
            Assert.AreEqual("{0} has invalid count ({1})!", validationResult.ValidationMessages.First().Message);
            Assert.AreEqual(nameof(Audiofile.Tracks), validationResult.ValidationMessages.First().Parameter?.First().ToString());
        }

        [TestMethod]
        public void Validate_TracksWithSamePosition_ReturnsValidationStatusError()
        {
            // Arrange
            var track1 = new Track()
            {
                Position = 1
            };
            var track2 = new Track()
            {
                Position = 1
            };
            var audiofile = new Audiofile()
            {
                Tracks = [track1, track2]
            };
            // Act
            var validationResult = audiofile.Validate(nameof(Audiofile.Tracks));
            // Assert
            Assert.AreEqual(ValidationStatus.Error, validationResult.Status);
            Assert.AreEqual("{0} {1} '{2}' is used also by {3}({4},{5},{6},{7},{8}). Positions must be unique!", validationResult.ValidationMessages.First().Message);
        }

        [TestMethod]
        public void Validate_TracksOverlapping_ReturnsValidationStatusError()
        {
            // Arrange
            var track1 = new Track()
            {
                Position = 1,
                Begin = TimeSpan.Zero,
                End = new TimeSpan(0, 3, 45)
            };
            var track2 = new Track()
            {
                Position = 2,
                Begin = new TimeSpan(0, 3, 42),
                End = new TimeSpan(0, 6, 32)
            };
            var audiofile = new Audiofile()
            {
                Tracks = [track1, track2]
            };
            // Act
            var validationResult = audiofile.Validate(nameof(Audiofile.Tracks));
            // Assert
            Assert.AreEqual(ValidationStatus.Error, validationResult.Status);
            Assert.AreEqual("{0}({1},{2},{3},{4},{5}) is overlapping with {0}({6},{7},{8},{9},{10}). Please make shure the timeinterval is only used once!", validationResult.ValidationMessages.First().Message);
        }
    }
}
