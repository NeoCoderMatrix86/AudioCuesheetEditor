using Microsoft.VisualStudio.TestTools.UnitTesting;
using AudioCuesheetEditor.Model.IO.Export;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AudioCuesheetEditor.Model.Entity;

namespace AudioCuesheetEditor.Model.IO.Export.Tests
{
    [TestClass()]
    public class SplitPointTests
    {
        [TestMethod()]
        public void ValidationTest()
        {
            var splitPoint = new SplitPoint();
            Assert.IsNull(splitPoint.Moment);
            var validationResult = splitPoint.Validate(x => x.Moment);
            Assert.AreEqual(ValidationStatus.Error, validationResult.Status);
            splitPoint.Moment = new TimeSpan(0, 30, 0);
            validationResult = splitPoint.Validate(x => x.Moment);
            Assert.AreEqual(ValidationStatus.Success, validationResult.Status);
        }
    }
}