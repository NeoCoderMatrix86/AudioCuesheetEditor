using AudioCuesheetEditor.Model.AudioCuesheet;
using AudioCuesheetEditor.Model.IO.Audio;
using AudioCuesheetEditor.Model.IO.Import;

namespace AudioCuesheetEditor.Services.UI
{
    public interface ISessionStateContainer
    {
        public event EventHandler? CuesheetChanged;
        public event EventHandler? ImportCuesheetChanged;
        public Cuesheet Cuesheet { get; set; }
        public Cuesheet? ImportCuesheet { get; set; }
        public Audiofile? ImportAudiofile { get; set; }
        public IImportfile? Importfile { get; set; }
        public void ResetImport();
    }
}
