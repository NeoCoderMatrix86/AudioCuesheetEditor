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
            var validationResult = section.Validate(x => x.Begin);
            Assert.AreEqual(ValidationStatus.Error, validationResult.Status);
            section.Begin = new TimeSpan(0, 30, 0);
            validationResult = section.Validate(x => x.Begin);
            Assert.AreEqual(ValidationStatus.Success, validationResult.Status);
            cuesheet.AddTrack(new Track() { Position = 1});
            cuesheet.AddTrack(new Track() { Position = 2, Begin = new TimeSpan(0, 8, 43), End = new TimeSpan(0, 15, 43) });
            validationResult = section.Validate(x => x.Begin);
            Assert.AreEqual(ValidationStatus.Error, validationResult.Status);
            section.Begin = new TimeSpan(0, 15, 43);
            validationResult = section.Validate(x => x.Begin);
            Assert.AreEqual(ValidationStatus.Success, validationResult.Status);
        }
    }
}