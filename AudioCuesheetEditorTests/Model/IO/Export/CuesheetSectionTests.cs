using Microsoft.VisualStudio.TestTools.UnitTesting;
using AudioCuesheetEditor.Model.IO.Export;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AudioCuesheetEditor.Model.Entity;
using AudioCuesheetEditor.Model.AudioCuesheet;

namespace AudioCuesheetEditor.Model.IO.Export.Tests
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
            var beginValidationResult = section.Validate(x => x.Begin);
            var endValidationResult = section.Validate(x => x.End);
            Assert.AreEqual(ValidationStatus.Error, beginValidationResult.Status);
            Assert.AreEqual(ValidationStatus.Error, endValidationResult.Status);
            section.Begin = new TimeSpan(0, 0, 0);
            beginValidationResult = section.Validate(x => x.Begin);
            endValidationResult = section.Validate(x => x.End);
            Assert.AreEqual(ValidationStatus.Success, beginValidationResult.Status);
            Assert.AreEqual(ValidationStatus.Error, endValidationResult.Status);
            cuesheet.AddTrack(new Track() { Position = 1, Begin = new TimeSpan(0, 0, 10) });
            cuesheet.AddTrack(new Track() { Position = 2, Begin = new TimeSpan(0, 8, 43), End = new TimeSpan(0, 15, 43) });
            beginValidationResult = section.Validate(x => x.Begin);
            endValidationResult = section.Validate(x => x.End);
            Assert.AreEqual(ValidationStatus.Error, beginValidationResult.Status);
            Assert.AreEqual(ValidationStatus.Error, endValidationResult.Status);
            section.Begin = new TimeSpan(0, 0, 10);
            beginValidationResult = section.Validate(x => x.Begin);
            endValidationResult = section.Validate(x => x.End);
            Assert.AreEqual(ValidationStatus.Success, beginValidationResult.Status);
            Assert.AreEqual(ValidationStatus.Error, endValidationResult.Status);
            section.Begin = new TimeSpan(0, 10, 0);
            section.End = new TimeSpan(0, 5, 0);
            beginValidationResult = section.Validate(x => x.Begin);
            endValidationResult = section.Validate(x => x.End);
            Assert.AreEqual(ValidationStatus.Error, beginValidationResult.Status);
            Assert.AreEqual(ValidationStatus.Error, endValidationResult.Status);
            section.Begin = new TimeSpan(0, 10, 0);
            section.End = new TimeSpan(0, 15, 0);
            beginValidationResult = section.Validate(x => x.Begin);
            endValidationResult = section.Validate(x => x.End);
            Assert.AreEqual(ValidationStatus.Success, beginValidationResult.Status);
            Assert.AreEqual(ValidationStatus.Success, endValidationResult.Status);
        }
    }
}