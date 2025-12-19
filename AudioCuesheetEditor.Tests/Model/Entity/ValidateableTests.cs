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

namespace AudioCuesheetEditor.Tests.Model.Entity
{
    public class ValidateableTestClass : Validateable
    {
        private string? testProperty;
        private int? testProperty2;

        public string? TestProperty
        {
            get => testProperty;
            set
            {
                testProperty = value;
                OnValidateablePropertyChanged();
            }
        }
        public int? TestProperty2
        {
            get => testProperty2;
            set
            {
                testProperty2 = value;
                OnValidateablePropertyChanged();
            }
        }
        public override ValidationResult Validate(string property)
        {
            ValidationStatus validationStatus = ValidationStatus.NoValidation;
            List<ValidationMessage>? validationMessages = null;
            switch (property)
            {
                case nameof(TestProperty):
                    validationStatus = ValidationStatus.Success;
                    if (string.IsNullOrEmpty(TestProperty))
                    {
                        validationMessages ??= [];
                        validationMessages.Add(new ValidationMessage("{0} has no value!", nameof(TestProperty)));
                    }
                    break;
            }
            return ValidationResult.Create(validationStatus, validationMessages);
        }
    }
    [TestClass()]
    public class ValidateableTests
    {
        [TestMethod()]
        public void ValidateTest()
        {
            var testObject = new ValidateableTestClass
            {
                TestProperty = string.Empty
            };
            Assert.AreEqual(ValidationStatus.Error, testObject.Validate().Status);
            Assert.IsNotNull(testObject.Validate().ValidationMessages);
            Assert.AreEqual(1, testObject.Validate().ValidationMessages?.Count);
            testObject.TestProperty = "Test";
            Assert.AreEqual(ValidationStatus.Success, testObject.Validate().Status);
        }
    }
}