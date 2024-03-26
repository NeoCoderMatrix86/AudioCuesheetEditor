//This file is part of AudioCuesheetEditor.

//AudioCuesheetEditor is free software: you can redistribute it and/or modify
//it under the terms of the GNU General Public License as published by
//the Free Software Foundation, either version 3 of the License, or
//(at your option) any later version.

//AudioCuesheetEditor is distributed in the hope that it will be useful,
//but WITHOUT ANY WARRANTY; without even the implied warranty of
//MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
//GNU General Public License for more details.

//You should have received a copy of the GNU General Public License
//along with Foobar.  If not, see
//<http: //www.gnu.org/licenses />.
using AudioCuesheetEditor.Model.IO.Audio;
using Blazorise;

namespace AudioCuesheetEditor.Model.Utility
{
    public class IOUtility
    {
        public static Boolean CheckFileMimeType(IFileEntry file, String mimeType, String fileExtension)
        {
            Boolean fileMimeTypeMatches = false;
            if ((file != null) && (String.IsNullOrEmpty(mimeType) == false) && (String.IsNullOrEmpty(fileExtension) == false))
            {
                if (String.IsNullOrEmpty(file.Type) == false)
                {
                    fileMimeTypeMatches = file.Type.ToLower() == mimeType.ToLower();
                }
                else
                {
                    //Try to find by file extension
                    var extension = Path.GetExtension(file.Name).ToLower();
                    fileMimeTypeMatches = extension == fileExtension.ToLower();
                }
            }
            return fileMimeTypeMatches;
        }

        public static Boolean CheckFileMimeTypeForAudioCodec(IFileEntry file)
        {
            return GetAudioCodec(file) != null;
        }

        public static AudioCodec? GetAudioCodec(IFileEntry fileEntry)
        {
            AudioCodec? foundAudioCodec = null;
            var extension = Path.GetExtension(fileEntry.Name).ToLower();
            // First search with mime type and file extension
            var audioCodecsFound = Audiofile.AudioCodecs.Where(x => x.MimeType.Equals(fileEntry.Type, StringComparison.OrdinalIgnoreCase) && x.FileExtension.Equals(extension, StringComparison.OrdinalIgnoreCase));
            if (audioCodecsFound.Count() <= 1)
            {
                foundAudioCodec = audioCodecsFound.FirstOrDefault();
            }
            else
            {
                // Second search with mime type or file extension
                audioCodecsFound = Audiofile.AudioCodecs.Where(x => x.MimeType.Equals(fileEntry.Type, StringComparison.OrdinalIgnoreCase) || x.FileExtension.Equals(extension, StringComparison.OrdinalIgnoreCase));
                foundAudioCodec = audioCodecsFound.FirstOrDefault();
            }
            return foundAudioCodec;
        }
    }
}
