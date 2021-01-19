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
using Microsoft.VisualStudio.TestTools.UnitTesting;
using AudioCuesheetEditor.Controller;
using System;
using System.Collections.Generic;
using System.Text;
using AudioCuesheetEditorTests.Utility;
using AudioCuesheetEditor.Model.AudioCuesheet;
using AudioCuesheetEditor.Model.Reflection;

namespace AudioCuesheetEditor.Controller.Tests
{
    [TestClass()]
    public class CuesheetControllerTests
    {
        [TestMethod()]
        public void GetFieldIdentifierTest()
        {
            var testHelper = new TestHelper();
            var cuesheetController = testHelper.CuesheetController;
            var cuesheet = new Cuesheet();
            Assert.IsNotNull(cuesheet);
            var identifier = cuesheetController.GetFieldIdentifier(cuesheet, nameof(Cuesheet.Artist));
            Assert.IsTrue(identifier.StartsWith(String.Format("{0}.{1}", nameof(Cuesheet), nameof(Cuesheet.Artist))));
            Assert.AreEqual(cuesheetController.GetFieldIdentifier(cuesheet, nameof(Cuesheet.Artist)), identifier);
            var fieldReference = FieldReference.Create(cuesheet, nameof(Cuesheet.Artist));
            Assert.AreEqual(cuesheetController.GetFieldIdentifier(fieldReference), identifier);
        }
    }
}