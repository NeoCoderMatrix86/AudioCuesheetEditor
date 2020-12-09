using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace AudioCuesheetEditor.Model.Entity
{
    public abstract class Validateable : IValidateable
    {
        protected List<ValidationError> validationErrors = new List<ValidationError>();

        public bool IsValid
        {
            get { return validationErrors.Count == 0; }
        }

        public IReadOnlyCollection<ValidationError> ValidationErrors
        {
            get { return validationErrors.AsReadOnly(); }
        }

        public String GetValidationErrors(String seperator = "<br />")
        {
            return String.Join(seperator, ValidationErrors.OrderBy(y => y.Type).Select(x => x.Message));
        }

        public event EventHandler ValidateablePropertyChanged;

        protected void OnValidateablePropertyChanged()
        {
            validationErrors.Clear();
            Validate();
            ValidateablePropertyChanged?.Invoke(this, EventArgs.Empty);
        }

        protected abstract void Validate();
    }
}
