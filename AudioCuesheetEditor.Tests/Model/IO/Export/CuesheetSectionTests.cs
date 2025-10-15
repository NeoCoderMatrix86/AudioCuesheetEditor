using AudioCuesheetEditor.Model.AudioCuesheet;
using AudioCuesheetEditor.Model.Entity;
using AudioCuesheetEditor.Model.IO.Export;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;

namespace AudioCuesheetEditor.Tests.Model.IO.Export
{
    [TestClass()]
    public class CuesheetSectionTests
    {
        [TestMethod()]
        public void ValidationTest()
        {
            var cuesheet = new Cuesheet();
            var section = new CuesheetSection(cuesheet);
            Assert.IsNull(section.Begin);
            var beginValidationResult = section.Validate(nameof(CuesheetSection.Begin));
            var endValidationResult = section.Validate(nameof(CuesheetSection.End));
            Assert.AreEqual(ValidationStatus.Error, beginValidationResult.Status);
            Assert.AreEqual(ValidationStatus.Error, endValidationResult.Status);
            section.Begin = new TimeSpan(0, 0, 0);
            beginValidationResult = section.Validate(nameof(CuesheetSection.Begin));
            endValidationResult = section.Validate(nameof(CuesheetSection.End));
            Assert.AreEqual(ValidationStatus.Success, beginValidationResult.Status);
            Assert.AreEqual(ValidationStatus.Error, endValidationResult.Status);
            cuesheet.AddTrack(new Track() { Position = 1, Begin = new TimeSpan(0, 0, 10) });
            cuesheet.AddTrack(new Track() { Position = 2, Begin = new TimeSpan(0, 8, 43), End = new TimeSpan(0, 15, 43) });
            beginValidationResult = section.Validate(nameof(CuesheetSection.Begin));
            endValidationResult = section.Validate(nameof(CuesheetSection.End));
            Assert.AreEqual(ValidationStatus.Error, beginValidationResult.Status);
            Assert.AreEqual(ValidationStatus.Error, endValidationResult.Status);
            section.Begin = new TimeSpan(0, 0, 10);
            beginValidationResult = section.Validate(nameof(CuesheetSection.Begin));
            endValidationResult = section.Validate(nameof(CuesheetSection.End));
            Assert.AreEqual(ValidationStatus.Success, beginValidationResult.Status);
            Assert.AreEqual(ValidationStatus.Error, endValidationResult.Status);
            section.Begin = new TimeSpan(0, 10, 0);
            section.End = new TimeSpan(0, 5, 0);
            beginValidationResult = section.Validate(nameof(CuesheetSection.Begin));
            endValidationResult = section.Validate(nameof(CuesheetSection.End));
            Assert.AreEqual(ValidationStatus.Error, beginValidationResult.Status);
            Assert.AreEqual(ValidationStatus.Error, endValidationResult.Status);
            section.Begin = new TimeSpan(0, 10, 0);
            section.End = new TimeSpan(0, 15, 0);
            beginValidationResult = section.Validate(nameof(CuesheetSection.Begin));
            endValidationResult = section.Validate(nameof(CuesheetSection.End));
            Assert.AreEqual(ValidationStatus.Success, beginValidationResult.Status);
            Assert.AreEqual(ValidationStatus.Success, endValidationResult.Status);
        }

        [TestMethod()]
        public void Begin_FirstSection_ReturnsTimespanZero()
        {
            // Arrange
            var cuesheet = new Cuesheet();
            cuesheet.AddTrack(new Track()
            {
                End = new TimeSpan(0, 4, 43)
            });
            cuesheet.AddTrack(new Track()
            {
                End = new TimeSpan(0, 9, 23)
            });
            cuesheet.AddTrack(new Track()
            {
                End = new TimeSpan(0, 14, 12)
            });
            cuesheet.AddTrack(new Track()
            {
                End = new TimeSpan(0, 18, 53)
            });
            cuesheet.AddTrack(new Track()
            {
                End = new TimeSpan(0, 22, 01)
            });
            cuesheet.AddTrack(new Track()
            {
                End = new TimeSpan(0, 25, 56)
            });
            cuesheet.AddTrack(new Track()
            {
                End = new TimeSpan(0, 31, 12)
            });
            // Act
            var section = cuesheet.AddSection();
            // Assert
            Assert.AreEqual(TimeSpan.Zero, section.Begin);
        }

        [TestMethod()]
        public void Begin_SecondSection_ReturnsFirstSectionEnd()
        {
            // Arrange
            var cuesheet = new Cuesheet();
            cuesheet.AddTrack(new Track()
            {
                End = new TimeSpan(0, 4, 43)
            });
            cuesheet.AddTrack(new Track()
            {
                End = new TimeSpan(0, 9, 23)
            });
            cuesheet.AddTrack(new Track()
            {
                End = new TimeSpan(0, 14, 12)
            });
            cuesheet.AddTrack(new Track()
            {
                End = new TimeSpan(0, 18, 53)
            });
            cuesheet.AddTrack(new Track()
            {
                End = new TimeSpan(0, 22, 01)
            });
            cuesheet.AddTrack(new Track()
            {
                End = new TimeSpan(0, 25, 56)
            });
            cuesheet.AddTrack(new Track()
            {
                End = new TimeSpan(0, 31, 12)
            });
            var section1 = cuesheet.AddSection();
            section1.End = new TimeSpan(0, 30, 0);
            // Act
            var section2 = cuesheet.AddSection();
            // Assert
            Assert.AreEqual(section1.End, section2.Begin);
        }

        [TestMethod()]
        public void Begin_ThirdSection_ReturnsSecondSectionEnd()
        {
            // Arrange
            var cuesheet = new Cuesheet();
            cuesheet.AddTrack(new Track()
            {
                End = new TimeSpan(0, 4, 43)
            });
            cuesheet.AddTrack(new Track()
            {
                End = new TimeSpan(0, 9, 23)
            });
            cuesheet.AddTrack(new Track()
            {
                End = new TimeSpan(0, 14, 12)
            });
            cuesheet.AddTrack(new Track()
            {
                End = new TimeSpan(0, 18, 53)
            });
            cuesheet.AddTrack(new Track()
            {
                End = new TimeSpan(0, 22, 01)
            });
            cuesheet.AddTrack(new Track()
            {
                End = new TimeSpan(0, 25, 56)
            });
            cuesheet.AddTrack(new Track()
            {
                End = new TimeSpan(0, 31, 12)
            });
            var section1 = cuesheet.AddSection();
            section1.End = new TimeSpan(0, 15, 0);
            var section2 = cuesheet.AddSection();
            section2.End = new TimeSpan(0, 30, 0);
            // Act
            var section3 = cuesheet.AddSection();
            // Assert
            Assert.AreEqual(section2.End, section3.Begin);
        }
    }
}