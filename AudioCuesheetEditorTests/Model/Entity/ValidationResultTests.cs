using AudioCuesheetEditor.Model.Entity;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AudioCuesheetEditorTests.Model.Entity
{
    [TestClass()]
    public class ValidationResultTests
    {
        [TestMethod()]
        public void StatusTest()
        {
            var result = new ValidationResult();
            Assert.IsNull(result.ErrorMessages);
            Assert.AreEqual(ValidationStatus.NoValidation, result.Status);
            result.Status = ValidationStatus.Success;
            Assert.IsNull(result.ErrorMessages);
            Assert.AreEqual(ValidationStatus.Success, result.Status);
        }

        [TestMethod()]
        public void ErrorTest()
        {
            var result = new ValidationResult();
            Assert.IsNull(result.ErrorMessages);
            Assert.AreEqual(ValidationStatus.NoValidation, result.Status);
            result.Status = ValidationStatus.Success;
            Assert.IsNull(result.ErrorMessages);
            Assert.AreEqual(ValidationStatus.Success, result.Status);
            result.ErrorMessages = new List<string>() { "Unit Test error1", "Unit Test error2" };
            Assert.IsNotNull(result.ErrorMessages);
            Assert.AreEqual(ValidationStatus.Error, result.Status);
        }
    }
}
