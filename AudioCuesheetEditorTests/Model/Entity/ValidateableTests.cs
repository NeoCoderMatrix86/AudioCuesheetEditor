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
using AudioCuesheetEditorTests.Utility;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;

namespace AudioCuesheetEditor.Model.Entity.Tests
{
    public class ValidateableTestClass : Validateable<ValidateableTestClass>
    {
        public String TestProperty { get; set; }
        //TODO
        protected override ValidationResult Validate(string property)
        {
            throw new NotImplementedException();
        }
    }
    [TestClass()]
    public class ValidateableTests
    {
        [TestMethod()]
        public void GetValidationErrorsTest()
        {
            var testObject = new ValidateableTestClass
            {
                TestProperty = String.Empty
            };
            var testhelper = new TestHelper();
            //TODO
            //Assert.IsNull(testObject.GetValidationErrors(testhelper.Localizer, validationErrorFilterType: ValidationErrorFilterType.ErrorOnly));
            //Assert.IsTrue(testObject.ValidationErrors.Count > 0);
            //Assert.IsNotNull(testObject.GetValidationErrors(testhelper.Localizer, nameof(ValidateableTestClass.TestProperty)));
        }
    }
}