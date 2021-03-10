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
using AudioCuesheetEditor.Controller;
using AudioCuesheetEditor.Model.IO;
using AudioCuesheetEditor.Model.IO.Audio;
using AudioCuesheetEditor.Model.IO.Export;
using AudioCuesheetEditor.Model.IO.Import;
using AudioCuesheetEditor.Shared;
using AudioCuesheetEditor.Shared.ResourceFiles;
using Microsoft.Extensions.Localization;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace AudioCuesheetEditor.Model.Options
{
    /// <summary>
    /// Enum for setting desired GUI mode
    /// </summary>
    public enum ViewMode
    {
        ViewModeFull = 0,
        ViewModeRecord = 1
    }

    public class ApplicationOptions
    {
        private String audioFileNameRecording;
        private String projectFilename;
        public ApplicationOptions()
        {
            SetDefaultValues();
        }

        public void SetDefaultValues()
        {
            //Declare defaults
            if (String.IsNullOrEmpty(CuesheetFileName) == true)
            {
                CuesheetFileName = CuesheetFile.DefaultFileName;
            }
            if (String.IsNullOrEmpty(CultureName) == true)
            {
                CultureName = OptionsController.DefaultCultureName;
            }
            if (ExportProfiles == null)
            {
                var list = new List<ExportProfile>();
                var exportProfile = new ExportProfile()
                {
                    FileName = "YouTube.txt",
                    Name = "YouTube"
                };
                exportProfile.SchemeHead.Scheme = "%Cuesheet.Artist% - %Cuesheet.Title%";
                exportProfile.SchemeTracks.Scheme = "%Track.Artist% - %Track.Title% %Track.Begin%";
                exportProfile.SchemeFooter.Scheme = String.Empty;
                list.Add(exportProfile);
                exportProfile = new ExportProfile()
                {
                    FileName = "Mixcloud.txt",
                    Name = "Mixcloud"
                };
                exportProfile.SchemeHead.Scheme = String.Empty;
                exportProfile.SchemeTracks.Scheme = "%Track.Artist% - %Track.Title% %Track.Begin%";
                exportProfile.SchemeFooter.Scheme = String.Empty;
                list.Add(exportProfile);
                exportProfile = new ExportProfile()
                {
                    FileName = "Export.csv",
                    Name = "CSV Export"
                };
                exportProfile.SchemeHead.Scheme = "%Cuesheet.Artist%;%Cuesheet.Title%;";
                exportProfile.SchemeTracks.Scheme = "%Track.Position%;%Track.Artist%;%Track.Title%;%Track.Begin%;%Track.End%;%Track.Length%";
                exportProfile.SchemeFooter.Scheme = String.Empty;
                list.Add(exportProfile);
                exportProfile = new ExportProfile()
                {
                    FileName = "Tracks.txt",
                    Name = "Tracks only"
                };
                exportProfile.SchemeHead.Scheme = String.Empty;
                exportProfile.SchemeTracks.Scheme = "%Track.Position% - %Track.Artist% - %Track.Title% - %Track.Begin% - %Track.End% - %Track.Length%";
                exportProfile.SchemeFooter.Scheme = String.Empty;
                list.Add(exportProfile);
                ExportProfiles = list.AsReadOnly();
            }
            if (TextImportScheme == null)
            {
                TextImportScheme = TextImportScheme.DefaultTextImportScheme;
            }
            if (String.IsNullOrEmpty(AudioFileNameRecording) == true)
            {
                AudioFileNameRecording = AudioFile.RecordingFileName;
            }
            if (LinkTracksWithPreviousOne.HasValue == false)
            {
                LinkTracksWithPreviousOne = true;
            }
            if (String.IsNullOrEmpty(ProjectFileName))
            {
                ProjectFileName = ProjectFile.DefaultFileName;
            }
        }
        public String CuesheetFileName { get; set; }
        public String CultureName { get; set; }
        [JsonIgnore]
        public CultureInfo Culture
        {
            get
            {
                if (String.IsNullOrEmpty(CultureName) == false)
                {
                    return new CultureInfo(CultureName);
                }
                else
                {
                    return CultureInfo.CurrentCulture;
                }
            }
        }
        public IReadOnlyCollection<ExportProfile> ExportProfiles { get; set; }
        public TextImportScheme TextImportScheme { get; set; }
        [JsonIgnore]
        public ViewMode ViewMode { get; set; }
        public String ViewModeName 
        {
            get { return Enum.GetName(typeof(ViewMode), ViewMode); }
            set { ViewMode = (ViewMode)Enum.Parse(typeof(ViewMode), value); }
        }
        public String AudioFileNameRecording 
        {
            get { return audioFileNameRecording; }
            set
            {
                if (String.IsNullOrEmpty(value) == false)
                {
                    var extension = Path.GetExtension(value);
                    if ((String.IsNullOrEmpty(extension)) || (extension.Equals(AudioFile.AudioCodecWEBM.FileExtension, StringComparison.OrdinalIgnoreCase) == false))
                    {
                        audioFileNameRecording = String.Format("{0}{1}", Path.GetFileNameWithoutExtension(value), AudioFile.AudioCodecWEBM.FileExtension);
                    }
                    else
                    {
                        audioFileNameRecording = value;
                    }
                }
                else
                {
                    audioFileNameRecording = null;
                }
            }
        }
        public Boolean? LinkTracksWithPreviousOne { get; set; }
        public String ProjectFileName 
        {
            get { return projectFilename; }
            set
            {
                if (String.IsNullOrEmpty(value) == false)
                {
                    var extension = Path.GetExtension(value);
                    if (extension.Equals(ProjectFile.FileExtension, StringComparison.OrdinalIgnoreCase))
                    {
                        projectFilename = value;
                    }
                    else
                    {
                        projectFilename = String.Format("{0}{1}", Path.GetFileNameWithoutExtension(value), ProjectFile.FileExtension);
                    }
                }
                else
                {
                    projectFilename = null;
                }
            }
        }
    }
}
