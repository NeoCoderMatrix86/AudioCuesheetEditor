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
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Collections.Generic;
using System.Linq;

namespace AudioCuesheetEditor.Tests.Model.Entity
{
    [TestClass()]
    public class ValidationResultTests
    {
        [TestMethod]
        public void Create_NoValidation_ReturnsExpectedResult()
        {
            // Arrange & Act
            var validationResult = ValidationResult.Create(ValidationStatus.NoValidation);

            // Assert
            Assert.AreEqual(ValidationStatus.NoValidation, validationResult.Status);
            Assert.AreEqual(0, validationResult.ValidationMessages.Count);
        }

        [TestMethod]
        public void Create_Success_ReturnsExpectedResult()
        {
            // Arrange & Act
            var validationResult = ValidationResult.Create(ValidationStatus.Success);

            // Assert
            Assert.AreEqual(ValidationStatus.Success, validationResult.Status);
            Assert.AreEqual(0, validationResult.ValidationMessages.Count);
        }

        [TestMethod]
        public void Create_Error_ReturnsExpectedResult()
        {
            // Arrange & Act
            var validationResult = ValidationResult.Create(ValidationStatus.Error);

            // Assert
            Assert.AreEqual(ValidationStatus.Error, validationResult.Status);
            Assert.AreEqual(0, validationResult.ValidationMessages.Count);
        }

        [TestMethod]
        public void Create_SuccessWithMessages_ReturnsErrorStatus()
        {
            // Arrange
            var validationMessages = new List<ValidationMessage> { new ValidationMessage("Testmessage!") };

            // Act
            var validationResult = ValidationResult.Create(ValidationStatus.Success, validationMessages);

            // Assert
            Assert.AreEqual(ValidationStatus.Error, validationResult.Status);
            CollectionAssert.AreEqual(validationMessages.ToList(), validationResult.ValidationMessages.ToList());
        }

        [TestMethod]
        public void Create_NoValidationWithMessages_ReturnsErrorStatus()
        {
            // Arrange
            var validationMessages = new List<ValidationMessage> { new("Testmessage!") };

            // Act
            var validationResult = ValidationResult.Create(ValidationStatus.NoValidation, validationMessages);

            // Assert
            Assert.AreEqual(ValidationStatus.Error, validationResult.Status);
            CollectionAssert.AreEqual(validationMessages.ToList(), validationResult.ValidationMessages.ToList());
        }

        [TestMethod]
        public void Default_Status_IsNoValidation()
        {
            // Arrange & Act
            var validationResult = new ValidationResult();

            // Assert
            Assert.AreEqual(ValidationStatus.NoValidation, validationResult.Status);
            Assert.AreEqual(0, validationResult.ValidationMessages.Count);
        }

        [TestMethod]
        public void Setting_ValidationMessages_SetsStatusToError()
        {
            // Arrange
            var validationResult = new ValidationResult();
            var messages = new List<ValidationMessage>
            {
                new("Unit Test error1"),
                new("Unit Test error2")
            };

            // Act
            validationResult.ValidationMessages = messages;

            // Assert
            Assert.IsNotNull(validationResult.ValidationMessages);
            Assert.AreEqual(ValidationStatus.Error, validationResult.Status);
        }
    }
}