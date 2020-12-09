using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace AudioCuesheetEditor.Model.IO
{
    public class AudioFile
    {
        public String FileName { get; private set; }
        public String AudioFileType
        {
            get { return Path.GetExtension(FileName).Replace(".", "").ToUpper(); }
        }
        public AudioFile(String fileName)
        {
            if (String.IsNullOrEmpty(fileName))
            {
                throw new ArgumentNullException(nameof(fileName));
            }
            FileName = fileName;
        }
    }
}
