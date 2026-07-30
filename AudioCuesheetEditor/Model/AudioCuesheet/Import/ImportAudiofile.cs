namespace AudioCuesheetEditor.Model.AudioCuesheet.Import
{
    public class ImportAudiofile
    {
        public String? Name { get; set;  }
        public IList<ImportTrack> Tracks { get; } = [];
    }
}
