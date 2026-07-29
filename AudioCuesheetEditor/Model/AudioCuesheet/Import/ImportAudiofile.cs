namespace AudioCuesheetEditor.Model.AudioCuesheet.Import
{
    public class ImportAudiofile
    {
        public String? Name { get; set;  }
        public ICollection<ImportTrack> Tracks { get; } = [];
    }
}
