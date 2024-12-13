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
using System.Collections.Generic;

namespace AudioCuesheetEditor.Model.Entity.Tests
{
    [TestClass()]
    public class ValidationResultTests
    {
        [TestMethod()]
        public void CreateTest()
        {
            var validationResult = ValidationResult.Create(ValidationStatus.NoValidation);
            Assert.AreEqual(ValidationStatus.NoValidation, validationResult.Status);
            Assert.IsNull(validationResult.ValidationMessages);
            validationResult = ValidationResult.Create(ValidationStatus.Success);
            Assert.AreEqual(ValidationStatus.Success, validationResult.Status);
            Assert.IsNull(validationResult.ValidationMessages);
            validationResult = ValidationResult.Create(ValidationStatus.Error);
            Assert.AreEqual(ValidationStatus.Error, validationResult.Status);
            Assert.IsNull(validationResult.ValidationMessages);
            var validationMessages = new List<ValidationMessage>() { new ValidationMessage("Testmessage!") };
            validationResult = ValidationResult.Create(ValidationStatus.Success, validationMessages);
            Assert.AreEqual(ValidationStatus.Error, validationResult.Status);
            Assert.IsNotNull(validationResult.ValidationMessages);
            validationResult = ValidationResult.Create(ValidationStatus.NoValidation, validationMessages);
            Assert.AreEqual(ValidationStatus.Error, validationResult.Status);
            Assert.IsNotNull(validationResult.ValidationMessages);
        }

        [TestMethod()]
        public void StatusTest()
        {
            var result = new ValidationResult();
            Assert.IsNull(result.ValidationMessages);
            Assert.AreEqual(ValidationStatus.NoValidation, result.Status);
            result.Status = ValidationStatus.Success;
            Assert.IsNull(result.ValidationMessages);
            Assert.AreEqual(ValidationStatus.Success, result.Status);
        }

        [TestMethod()]
        public void ErrorTest()
        {
            var result = new ValidationResult();
            Assert.IsNull(result.ValidationMessages);
            Assert.AreEqual(ValidationStatus.NoValidation, result.Status);
            result.Status = ValidationStatus.Success;
            Assert.IsNull(result.ValidationMessages);
            Assert.AreEqual(ValidationStatus.Success, result.Status);
            result.ValidationMessages = new List<ValidationMessage>() { new ValidationMessage("Unit Test error1"), new ValidationMessage("Unit Test error2") };
            Assert.IsNotNull(result.ValidationMessages);
            Assert.AreEqual(ValidationStatus.Error, result.Status);
        }
    }
}