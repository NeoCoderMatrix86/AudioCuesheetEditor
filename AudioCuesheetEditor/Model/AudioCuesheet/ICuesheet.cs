using AudioCuesheetEditor.Model.IO;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace AudioCuesheetEditor.Model.AudioCuesheet
{
    public interface ICuesheet : ICuesheetEntity
    {
        public String Artist { get; set; }
        public String Title { get; set; }
        public AudioFile AudioFile { get; set; }
    }
}
