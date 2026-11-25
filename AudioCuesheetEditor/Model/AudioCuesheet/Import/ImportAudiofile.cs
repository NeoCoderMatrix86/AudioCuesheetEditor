namespace AudioCuesheetEditor.Model.AudioCuesheet.Import;

public class ImportAudiofile : IAudiofile
{
    public string? Name { get; set;  }
    public IList<ImportTrack> Tracks { get; } = [];
}
