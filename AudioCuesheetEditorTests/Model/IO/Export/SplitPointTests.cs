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
    public class SplitPointTests
    {
        [TestMethod()]
        public void ValidationTest()
        {
            var cuesheet = new Cuesheet();
            var splitPoint = new SplitPoint(cuesheet);
            Assert.IsNull(splitPoint.Moment);
            var validationResult = splitPoint.Validate(x => x.Moment);
            Assert.AreEqual(ValidationStatus.Error, validationResult.Status);
            splitPoint.Moment = new TimeSpan(0, 30, 0);
            validationResult = splitPoint.Validate(x => x.Moment);
            Assert.AreEqual(ValidationStatus.Success, validationResult.Status);
            cuesheet.AddTrack(new Track() { Position = 1});
            cuesheet.AddTrack(new Track() { Position = 2, Begin = new TimeSpan(0, 8, 43), End = new TimeSpan(0, 15, 43) });
            validationResult = splitPoint.Validate(x => x.Moment);
            Assert.AreEqual(ValidationStatus.Error, validationResult.Status);
            splitPoint.Moment = new TimeSpan(0, 15, 43);
            validationResult = splitPoint.Validate(x => x.Moment);
            Assert.AreEqual(ValidationStatus.Success, validationResult.Status);
        }
    }
}