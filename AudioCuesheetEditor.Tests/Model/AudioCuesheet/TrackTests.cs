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

namespace AudioCuesheetEditor.Tests.Model.AudioCuesheet
{
    [TestClass()]
    public class TrackTests
    {
        [TestMethod()]
        public void Validate_PositionNull_ReturnsValidationStatusError()
        {
            // Arrange
            var track = new Track();
            // Act
            var endValidationResult = track.Validate(nameof(Track.Position));
            // Assert
            Assert.AreEqual(ValidationStatus.Error, endValidationResult.Status);
        }

        [TestMethod()]
        public void Validate_Position0_ReturnsValidationStatusError()
        {
            // Arrange
            var track = new Track()
            {
                Position = 0
            };
            // Act
            var endValidationResult = track.Validate(nameof(Track.Position));
            // Assert
            Assert.AreEqual(ValidationStatus.Error, endValidationResult.Status);
        }

        [TestMethod()]
        public void Validate_PositionOvverlapping_ReturnsValidationStatusError()
        {
            // Arrange
            var track1 = new Track()
            {
                Position = 1
            };
            var track = new Track()
            {
                Position = 1
            };
            var cuesheet = new Cuesheet()
            {
                Tracks = [track1, track]
            };
            track1.Cuesheet = cuesheet;
            track.Cuesheet = cuesheet;
            // Act
            var endValidationResult = track.Validate(nameof(Track.Position));
            // Assert
            Assert.AreEqual(ValidationStatus.Error, endValidationResult.Status);
        }

        [TestMethod()]
        public void Validate_InvalidEnd_ReturnsValidationStatusError()
        {
            // Arrange
            var track = new Track
            {
                Begin = new TimeSpan(0, 2, 32),
                End = new TimeSpan(0, 1, 15)
            };
            // Act
            var endValidationResult = track.Validate(nameof(Track.End));
            // Assert
            Assert.AreEqual(ValidationStatus.Error, endValidationResult.Status);
        }

        [TestMethod()]
        public void Validate_InvalidBegin_ReturnsValidationStatusError()
        {
            // Arrange
            var track = new Track
            {
                Begin = new TimeSpan(0, 4, 53),
                End = new TimeSpan(0, 3, 15)
            };
            // Act
            var endValidationResult = track.Validate(nameof(Track.Begin));
            // Assert
            Assert.AreEqual(ValidationStatus.Error, endValidationResult.Status);
        }
    }
}