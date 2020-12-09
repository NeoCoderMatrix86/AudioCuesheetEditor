using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace AudioCuesheetEditor.Model.Entity
{
    public interface IValidateable
    {
        public Boolean IsValid { get; }
        public IReadOnlyCollection<ValidationError> ValidationErrors { get; }
    }
}
