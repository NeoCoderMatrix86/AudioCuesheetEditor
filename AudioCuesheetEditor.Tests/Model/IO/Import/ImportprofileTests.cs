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

using AudioCuesheetEditor.Model.Entity;
using AudioCuesheetEditor.Model.IO.Import;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Linq;

namespace AudioCuesheetEditor.Tests.Model.IO.Import
{
    [TestClass()]
    public class ImportprofileTests
    {
        [TestMethod()]
        public void Validate_WithoutPlaceholder_ReturnsInvalid()
        {
            // Arrange
            var importprofile = new Importprofile()
            {
                SchemeCuesheet = "Test 1",
                SchemeTracks = "Here comes next track"
            };
            // Act
            var result = importprofile.Validate();
            // Assert
            Assert.AreEqual(ValidationStatus.Error, result.Status);
            Assert.HasCount(2, result.ValidationMessages);
            Assert.AreEqual("{0} contains no placeholder!", result.ValidationMessages.First().Message);
            Assert.AreEqual(nameof(Importprofile.SchemeCuesheet), result.ValidationMessages.First().Parameter?.First().ToString());
            Assert.AreEqual("{0} contains no placeholder!", result.ValidationMessages.Last().Message);
            Assert.AreEqual(nameof(Importprofile.SchemeTracks), result.ValidationMessages.Last().Parameter?.First().ToString());
        }
    }
}