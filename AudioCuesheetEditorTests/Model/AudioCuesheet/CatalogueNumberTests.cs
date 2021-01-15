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
using AudioCuesheetEditor.Model.AudioCuesheet;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AudioCuesheetEditorTests.Utility;

namespace AudioCuesheetEditor.Model.AudioCuesheet.Tests
{
    [TestClass()]
    public class CatalogueNumberTests
    {
        [TestMethod()]
        public void CatalogueNumberTest()
        {
            var catalogueNumber = new CatalogueNumber();
            Assert.IsFalse(catalogueNumber.IsValid);
            Assert.IsTrue(catalogueNumber.ValidationErrors.Count == 1);
            catalogueNumber.Value = "Testvalue";
            Assert.IsFalse(catalogueNumber.IsValid);
            catalogueNumber.Value = "01234567891234567890";
            Assert.IsFalse(catalogueNumber.IsValid);
            catalogueNumber.Value = "1234567890123";
            Assert.IsTrue(catalogueNumber.IsValid);
        }
    }
}