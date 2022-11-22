using Microsoft.VisualStudio.TestTools.UnitTesting;
using AudioCuesheetEditor.Extensions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AudioCuesheetEditorTests.Utility;
using AudioCuesheetEditor.Model.UI;
using AudioCuesheetEditor.Model.AudioCuesheet;

namespace AudioCuesheetEditor.Extensions.Tests
{
    [TestClass()]
    public class SessionStateContainerTests
    {
        [TestMethod()]
        public void SessionStateContainerTest()
        {
            var helper = new TestHelper();
            var manager = new TraceChangeManager(TestHelper.CreateLogger<TraceChangeManager>());
            var container = new SessionStateContainer(manager);
            var importCuesheetChangedFired = false;
            container.ImportCuesheetChanged += delegate
            {
                importCuesheetChangedFired = true;
            };
            container.ImportCuesheet = null;
            Assert.IsFalse(importCuesheetChangedFired);
            container.ImportCuesheet = new Cuesheet();
            Assert.IsTrue(importCuesheetChangedFired);
        }
    }
}