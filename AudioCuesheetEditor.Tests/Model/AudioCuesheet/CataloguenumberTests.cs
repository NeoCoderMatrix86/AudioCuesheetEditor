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

namespace AudioCuesheetEditor.Tests.Model.AudioCuesheet
{
    [TestClass()]
    public class CataloguenumberTests
    {
        [TestMethod()]
        public void CatalogueNumberTest()
        {
            var catalogueNumber = new Cataloguenumber();
            Assert.AreEqual(ValidationStatus.NoValidation, catalogueNumber.Validate().Status);
            catalogueNumber.Value = "Testvalue";
            Assert.AreEqual(ValidationStatus.Error, catalogueNumber.Validate().Status);
            catalogueNumber.Value = "01234567891234567890";
            Assert.AreEqual(ValidationStatus.Error, catalogueNumber.Validate().Status);
            catalogueNumber.Value = "1234567890123";
            Assert.AreEqual(ValidationStatus.Success, catalogueNumber.Validate().Status);
        }
    }
}