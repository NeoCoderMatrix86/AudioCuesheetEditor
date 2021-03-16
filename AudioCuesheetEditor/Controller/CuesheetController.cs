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
using AudioCuesheetEditor.Model.Entity;
using AudioCuesheetEditor.Model.IO.Audio;
using AudioCuesheetEditor.Model.Reflection;
using Microsoft.AspNetCore.Components.Forms;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace AudioCuesheetEditor.Controller
{
    public class CuesheetController
    {
        private readonly Dictionary<FieldReference, Guid> fieldIdentifier = new Dictionary<FieldReference, Guid>();

        public String GetFieldIdentifier(IValidateable validateable, String property)
        {
            if (validateable == null)
            {
                throw new ArgumentNullException(nameof(validateable));
            }
            var identifier = fieldIdentifier.FirstOrDefault(x => x.Key.Owner == validateable && x.Key.Property == property);
            if (identifier.Key == null)
            {
                fieldIdentifier.Add(FieldReference.Create(validateable, property), Guid.NewGuid());
                identifier = fieldIdentifier.FirstOrDefault(x => x.Key.Owner == validateable && x.Key.Property == property);
            }
            return String.Format("{0}_{1}", identifier.Key.CompleteName, identifier.Value.ToString());
        }

        public String GetFieldIdentifier(FieldReference fieldReference)
        {
            if (fieldReference == null)
            {
                throw new ArgumentNullException(nameof(fieldReference));
            }
            var identifier = fieldIdentifier.FirstOrDefault(x => x.Key == fieldReference);
            if (identifier.Key == null)
            {
                fieldIdentifier.Add(fieldReference, Guid.NewGuid());
                identifier = fieldIdentifier.FirstOrDefault(x => x.Key == fieldReference);
            }
            return String.Format("{0}_{1}", identifier.Key.CompleteName, identifier.Value.ToString());
        }

        public static Boolean CheckFileMimeType(IBrowserFile file, String mimeType, String fileExtension)
        {
            Boolean fileMimeTypeMatches = false;
            if ((file != null) && (String.IsNullOrEmpty(mimeType) == false) && (String.IsNullOrEmpty(fileExtension) == false))
            {
                if (String.IsNullOrEmpty(file.ContentType) == false)
                {
                    fileMimeTypeMatches = file.ContentType.ToLower() == mimeType.ToLower();
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

        public static Boolean CheckFileMimeType(IBrowserFile file, IReadOnlyCollection<AudioCodec> audioCodecs)
        {
            Boolean fileMimeTypeMatches = false;
            if ((file != null) && (audioCodecs != null))
            {
                var extension = Path.GetExtension(file.Name).ToLower();
                var audioCodecsFound = audioCodecs.Where(x => x.MimeType.Equals(file.ContentType, StringComparison.OrdinalIgnoreCase) || x.FileExtension.Equals(extension, StringComparison.OrdinalIgnoreCase));
                fileMimeTypeMatches = (audioCodecsFound != null) && (audioCodecsFound.Any());
            }
            return fileMimeTypeMatches;
        }
    }
}
