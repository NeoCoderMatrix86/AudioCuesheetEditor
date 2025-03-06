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
using AudioCuesheetEditor.Services.UI;
using AudioCuesheetEditor.Tests.Utility;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace AudioCuesheetEditor.Tests.Extensions
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