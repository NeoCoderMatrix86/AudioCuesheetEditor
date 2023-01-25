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

        private String recordedAudiofilename;
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

        public ApplicationOptions()
        {
            cuesheetFilename = Cuesheetfile.DefaultFilename;
            CultureName = DefaultCultureName;
            recordedAudiofilename = Audiofile.RecordingFileName;
            LinkTracksWithPreviousOne = true;
            projectFilename = Projectfile.DefaultFilename;
            RecordCountdownTimer = 5;
        }

        public String? CuesheetFilename 
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
        public String? ViewModename 
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
        public String RecordedAudiofilename 
        {
            get { return recordedAudiofilename; }
            set
            {
                if (String.IsNullOrEmpty(value) == false)
                {
                    var extension = Path.GetExtension(value);
                    if (String.IsNullOrEmpty(extension) || (extension.Equals(Audiofile.AudioCodecWEBM.FileExtension, StringComparison.OrdinalIgnoreCase) == false))
                    {
                        recordedAudiofilename = String.Format("{0}{1}", Path.GetFileNameWithoutExtension(value), Audiofile.AudioCodecWEBM.FileExtension);
                    }
                    else
                    {
                        recordedAudiofilename = value;
                    }
                }
                else
                {
                    recordedAudiofilename = String.Empty;
                }
            }
        }
        public Boolean? LinkTracksWithPreviousOne { get; set; }
        public String? ProjectFilename 
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
        public String? RecordTimeSensitivityname
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
