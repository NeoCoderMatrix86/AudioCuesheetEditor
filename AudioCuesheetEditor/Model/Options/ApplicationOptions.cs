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
using AudioCuesheetEditor.Model.AudioCuesheet;
using AudioCuesheetEditor.Model.IO;
using AudioCuesheetEditor.Model.IO.Audio;
using AudioCuesheetEditor.Model.Utility;
using System.Globalization;
using System.Text.Json.Serialization;

namespace AudioCuesheetEditor.Model.Options
{
    /// <summary>
    /// Enum for setting desired GUI mode
    /// </summary>
    public enum ViewMode
    {
        ViewModeFull = 0,
        ViewModeRecord = 1,
        ViewModeImport = 2
    }

    public enum TimeSensitivityMode
    {
        Full = 0,
        Seconds = 1,
        Minutes = 2
    }

    public class ApplicationOptions : IOptions
    {
        public const String DefaultCultureName = "en-US";

        private String audioFileNameRecording;
        private String? projectFilename;
        private String? cuesheetFilename;

        public static IReadOnlyCollection<CultureInfo> AvailableCultures
        {
            get
            {
                var cultures = new List<CultureInfo>
                {
                    new CultureInfo("en-US"),
                    new CultureInfo("de-DE")
                };
                return cultures.AsReadOnly();
            }
        }

#pragma warning disable CS8618 // Ein Non-Nullable-Feld muss beim Beenden des Konstruktors einen Wert ungleich NULL enthalten. Erwägen Sie die Deklaration als Nullable.
        public ApplicationOptions()
#pragma warning restore CS8618 // Ein Non-Nullable-Feld muss beim Beenden des Konstruktors einen Wert ungleich NULL enthalten. Erwägen Sie die Deklaration als Nullable.
        {
            SetDefaultValues();
        }

        public void SetDefaultValues()
        {
            //Declare defaults
            if (String.IsNullOrEmpty(CuesheetFileName) == true)
            {
                CuesheetFileName = Cuesheetfile.DefaultFileName;
            }
            if (String.IsNullOrEmpty(CultureName) == true)
            {
                CultureName = DefaultCultureName;
            }
            if (String.IsNullOrEmpty(AudioFileNameRecording) == true)
            {
                AudioFileNameRecording = Audiofile.RecordingFileName;
            }
            if (LinkTracksWithPreviousOne.HasValue == false)
            {
                LinkTracksWithPreviousOne = true;
            }
            if (String.IsNullOrEmpty(ProjectFileName))
            {
                ProjectFileName = Projectfile.DefaultFileName;
            }
            if (RecordCountdownTimer.HasValue == false)
            {
                RecordCountdownTimer = 5;
            }
            
        }
        public String? CuesheetFileName 
        {
            get => cuesheetFilename;
            set
            {
                if (String.IsNullOrEmpty(value) == false)
                {
                    var extension = Path.GetExtension(value);
                    if (extension.Equals(Cuesheet.FileExtension, StringComparison.OrdinalIgnoreCase))
                    {
                        cuesheetFilename = value;
                    }
                    else
                    {
                        cuesheetFilename = String.Format("{0}{1}", Path.GetFileNameWithoutExtension(value), Cuesheet.FileExtension);
                    }
                }
                else
                {
                    cuesheetFilename = null;
                }
            }
        }
        public String? CultureName { get; set; }
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
        [JsonIgnore]
        public ViewMode ViewMode { get; set; }
        public String? ViewModeName 
        {
            get { return Enum.GetName(typeof(ViewMode), ViewMode); }
            set 
            {
                if (value != null)
                {
                    ViewMode = (ViewMode)Enum.Parse(typeof(ViewMode), value);
                }
                else
                {
                    throw new ArgumentNullException(nameof(value));
                }
            }
        }
        public String AudioFileNameRecording 
        {
            get { return audioFileNameRecording; }
            set
            {
                if (String.IsNullOrEmpty(value) == false)
                {
                    var extension = Path.GetExtension(value);
                    if ((String.IsNullOrEmpty(extension)) || (extension.Equals(Audiofile.AudioCodecWEBM.FileExtension, StringComparison.OrdinalIgnoreCase) == false))
                    {
                        audioFileNameRecording = String.Format("{0}{1}", Path.GetFileNameWithoutExtension(value), Audiofile.AudioCodecWEBM.FileExtension);
                    }
                    else
                    {
                        audioFileNameRecording = value;
                    }
                }
                else
                {
                    audioFileNameRecording = String.Empty;
                }
            }
        }
        public Boolean? LinkTracksWithPreviousOne { get; set; }
        public String? ProjectFileName 
        {
            get { return projectFilename; }
            set
            {
                if (String.IsNullOrEmpty(value) == false)
                {
                    var extension = Path.GetExtension(value);
                    if (extension.Equals(Projectfile.FileExtension, StringComparison.OrdinalIgnoreCase))
                    {
                        projectFilename = value;
                    }
                    else
                    {
                        projectFilename = String.Format("{0}{1}", Path.GetFileNameWithoutExtension(value), Projectfile.FileExtension);
                    }
                }
                else
                {
                    projectFilename = null;
                }
            }
        }
        public int? RecordCountdownTimer { get; set; }

        [JsonIgnore]
        public TimeSensitivityMode RecordTimeSensitivity { get; set; }
        public String? RecordTimeSensitivityName
        {
            get { return Enum.GetName(typeof(TimeSensitivityMode), RecordTimeSensitivity); }
            set 
            {
                if (value != null)
                {
                    RecordTimeSensitivity = (TimeSensitivityMode)Enum.Parse(typeof(TimeSensitivityMode), value);
                }
                else
                {
                    throw new ArgumentNullException(nameof(value));
                }
            }
        }

        public TimeSpanFormat? TimeSpanFormat { get; set; }
    }
}
