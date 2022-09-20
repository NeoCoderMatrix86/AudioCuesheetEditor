using AudioCuesheetEditor.Model.Entity;

namespace AudioCuesheetEditor.Model.Utility
{
    public class Timespanformat : Validateable
    {
        public const String Days = "Days";
        public const String Hours = "Hours";
        public const String Minutes = "Minutes";
        public const String Seconds = "Seconds";
        public const String Milliseconds = "Milliseconds";

        private string? scheme;

        public String? Scheme
        {
            get => scheme;
            set
            {
                scheme = value;
                OnValidateablePropertyChanged();
            }
        }

        protected override void Validate()
        {
            //TODO
            //throw new NotImplementedException();
        }
    }
}
