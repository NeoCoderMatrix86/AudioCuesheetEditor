using System.Text.Json.Serialization;

namespace AudioCuesheetEditor.Model.Options
{
    public enum ViewMode
    {
        DetailView = 0,
        RecordView = 1,
        ImportView = 2
    }
    public class ViewOptions : IOptions
    {
        [JsonIgnore]
        public ViewMode ActiveTab { get; set; }
        public String? ActiveTabName
        {
            get => Enum.GetName(ActiveTab);
            set
            {
                if (value != null)
                {
                    ActiveTab = Enum.Parse<ViewMode>(value);
                }
                else
                {
                    throw new ArgumentNullException(nameof(value));
                }
            }
        }
    }
}
