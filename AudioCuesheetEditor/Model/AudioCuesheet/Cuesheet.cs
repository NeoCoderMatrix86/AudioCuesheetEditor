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
using AudioCuesheetEditor.Model.Entity;
using AudioCuesheetEditor.Model.IO;
using AudioCuesheetEditor.Model.IO.Audio;
using AudioCuesheetEditor.Model.IO.Import;
using AudioCuesheetEditor.Model.Options;
using AudioCuesheetEditor.Model.Reflection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace AudioCuesheetEditor.Model.AudioCuesheet
{
    public enum MoveDirection
    {
        Up,
        Down
    }

    public class Cuesheet : Validateable, ICuesheet<Track>
    {
        private readonly object syncLock = new object();

        private readonly List<Track> tracks;
        private String artist;
        private String title;
        private AudioFile audioFile;
        private CDTextfile cDTextfile;
        private DateTime? recordingStart;
        private Boolean currentlyHandlingRankPropertyValueChanged;

        public event EventHandler AudioFileChanged;
        public Cuesheet()
        {
            tracks = new List<Track>();
            CatalogueNumber = new CatalogueNumber();
            CatalogueNumber.ValidateablePropertyChanged += CatalogueNumber_ValidateablePropertyChanged;
            Validate();
        }

        public IReadOnlyCollection<Track> Tracks => tracks.AsReadOnly();
        
        public String Artist 
        {
            get { return artist; }
            set { artist = value; OnValidateablePropertyChanged(); }
        }
        public String Title 
        {
            get { return title; }
            set { title = value; OnValidateablePropertyChanged(); }
        }
        public AudioFile AudioFile 
        {
            get { return audioFile; }
            set { audioFile = value; OnValidateablePropertyChanged(); AudioFileChanged?.Invoke(this, EventArgs.Empty); }
        }

        public CDTextfile CDTextfile 
        {
            get { return cDTextfile; }
            set { cDTextfile = value; OnValidateablePropertyChanged(); }
        }

        public CatalogueNumber CatalogueNumber { get; private set; }

        public Boolean CanWriteCuesheetFile
        {
            get
            {
                var cuesheetFile = new CuesheetFile(this);
                return cuesheetFile.IsExportable;
            }
        }

        public bool IsRecording
        {
            get { return RecordingTime.HasValue; }
        }

        public TimeSpan? RecordingTime
        {
            get 
            { 
                if (recordingStart.HasValue == true)
                {
                    return DateTime.UtcNow - recordingStart;
                }
                else
                {
                    return null;
                }
            }
        }

        public void AddTrack(Track track, ApplicationOptions applicationOptions)
        {
            if (track == null)
            {
                throw new ArgumentNullException(nameof(track));
            }
            if (applicationOptions == null)
            {
                throw new ArgumentNullException(nameof(applicationOptions));
            }
            if (track.IsCloned)
            {
                throw new ArgumentException("Cloned tracks may not be added!");
            }
            if (IsRecording)
            {
                track.Begin = DateTime.UtcNow - recordingStart.Value;
            }
            track.Cuesheet = this;
            tracks.Add(track);
            ReCalculateTrackProperties(track);
            if ((applicationOptions.LinkTracksWithPreviousOne.HasValue) && (applicationOptions.LinkTracksWithPreviousOne.Value == true))
            {
                if (tracks.Count > 1)
                {
                    var previousTrack = tracks.ElementAt(tracks.IndexOf(track) - 1);
                    track.LinkedPreviousTrack = previousTrack;
                }
            }
            track.RankPropertyValueChanged += Track_RankPropertyValueChanged;
            OnValidateablePropertyChanged();
        }

        public void RemoveTrack(Track track)
        {
            if (track == null)
            {
                throw new ArgumentNullException(nameof(track));
            }
            tracks.Remove(track);
            track.Cuesheet = null;
            track.RankPropertyValueChanged -= Track_RankPropertyValueChanged;
            OnValidateablePropertyChanged();
        }

        public void RemoveAllTracks()
        {
            tracks.Clear();
            OnValidateablePropertyChanged();
        }

        public Boolean MoveTrackPossible(Track track, MoveDirection moveDirection)
        {
            Boolean movePossible = false;
            if (track == null)
            {
                throw new ArgumentNullException(nameof(track));
            }
            lock (syncLock)
            {
                var index = Tracks.ToList().IndexOf(track);
                if (moveDirection == MoveDirection.Up)
                {
                    if (index > 0)
                    {
                        movePossible = true;
                    }
                }
                if (moveDirection == MoveDirection.Down)
                {
                    if ((index + 1) < Tracks.Count)
                    {
                        movePossible = true;
                    }
                }
            }
            return movePossible;
        }

        public void MoveTrack(Track track, MoveDirection moveDirection)
        {
            if (track == null)
            {
                throw new ArgumentNullException(nameof(track));
            }
            var index = tracks.IndexOf(track);
            if (moveDirection == MoveDirection.Up)
            {
                if (index > 0)
                {
                    var currentTrack = tracks.ElementAt(index - 1);
                    tracks[index - 1] = track;
                    tracks[index] = currentTrack;
                }
            }
            if (moveDirection == MoveDirection.Down)
            {
                if ((index + 1) < Tracks.Count)
                {
                    var currentTrack = tracks.ElementAt(index + 1);
                    tracks[index + 1] = track;
                    tracks[index] = currentTrack;
                }
            }
        }

        public void Import(TextImportFile textImportFile, ApplicationOptions applicationOptions)
        {
            if (textImportFile == null)
            {
                throw new ArgumentNullException(nameof(textImportFile));
            }
            if (applicationOptions == null)
            {
                throw new ArgumentNullException(nameof(applicationOptions));
            }
            if (textImportFile.IsValid == false)
            {
                throw new InvalidOperationException(String.Format("{0} was not valid!", nameof(textImportFile)));
            }
            CopyValues(textImportFile.ImportCuesheet, applicationOptions);
        }

        public void StartRecording()
        {
            recordingStart = DateTime.UtcNow;
        }

        public void StopRecording()
        {
            //Set end of last track
            var lastTrack = Tracks.LastOrDefault();
            if (lastTrack != null)
            {
                lastTrack.End = DateTime.UtcNow - recordingStart.Value;
            }
            recordingStart = null;
        }

        protected override void Validate()
        {
            if (String.IsNullOrEmpty(Artist) == true)
            {
                validationErrors.Add(new ValidationError(FieldReference.Create(this, nameof(Artist)), ValidationErrorType.Warning, "HasNoValue", nameof(Artist)));
            }
            if (String.IsNullOrEmpty(Title) == true)
            {
                validationErrors.Add(new ValidationError(FieldReference.Create(this, nameof(Title)), ValidationErrorType.Warning, "HasNoValue", nameof(Title)));
            }
            if (AudioFile == null)
            {
                validationErrors.Add(new ValidationError(FieldReference.Create(this, nameof(AudioFile)), ValidationErrorType.Error, "HasNoValue", nameof(AudioFile)));
            }
            if (tracks.Count < 1)
            {
                validationErrors.Add(new ValidationError(FieldReference.Create(this, nameof(Tracks)), ValidationErrorType.Error, "HasInvalidCount", nameof(Tracks), 0));
            }
            if (CDTextfile == null)
            {
                validationErrors.Add(new ValidationError(FieldReference.Create(this, nameof(CDTextfile)), ValidationErrorType.Warning, "HasNoValue", nameof(CDTextfile)));
            }
            if (CatalogueNumber == null)
            {
                validationErrors.Add(new ValidationError(FieldReference.Create(this, nameof(CatalogueNumber)), ValidationErrorType.Warning, "HasNoValue", nameof(CatalogueNumber)));
            }
            else
            {
                _ = CatalogueNumber.IsValid;
                validationErrors.AddRange(CatalogueNumber.ValidationErrors);
            }
        }

        private void Track_RankPropertyValueChanged(object sender, string e)
        {
            if (currentlyHandlingRankPropertyValueChanged == false)
            {
                currentlyHandlingRankPropertyValueChanged = true;
                Track trackRaisedEvent = (Track)sender;
                var index = tracks.IndexOf(trackRaisedEvent);
                switch (e)
                {
                    case nameof(Track.Begin):
                        if (index > 0)
                        {
                            var previousTrack = tracks.ElementAt(index - 1);
                            if ((previousTrack != trackRaisedEvent) && (previousTrack.End.HasValue == false))
                            {
                                previousTrack.End = trackRaisedEvent.Begin;
                            }
                        }
                        break;
                    case nameof(Track.End):
                        if ((index + 1) < Tracks.Count)
                        {
                            var nextTrack = tracks.ElementAt(index + 1);
                            if ((nextTrack != trackRaisedEvent) && (nextTrack.Begin.HasValue == false))
                            {
                                nextTrack.Begin = trackRaisedEvent.End;
                            }
                        }
                        break;
                }
                currentlyHandlingRankPropertyValueChanged = false;
            }
        }

        private void CatalogueNumber_ValidateablePropertyChanged(object sender, EventArgs e)
        {
            OnValidateablePropertyChanged();
        }

        private void ReCalculateTrackProperties(Track trackToCalculate)
        {
            if ((AudioFile != null) && (AudioFile.Duration.HasValue) && (trackToCalculate.End.HasValue == false))
            {
                trackToCalculate.End = AudioFile.Duration;
            }
            if (Tracks.Count > 1)
            {
                var lastTrack = tracks.ElementAt(tracks.IndexOf(trackToCalculate) - 1);
                if (lastTrack != trackToCalculate)
                {
                    if ((AudioFile != null) && (AudioFile.Duration.HasValue) && (lastTrack.End.HasValue) && (lastTrack.End.Value == AudioFile.Duration.Value))
                    {
                        lastTrack.End = null;
                    }
                    if (trackToCalculate.Position.HasValue == false)
                    {
                        trackToCalculate.Position = lastTrack.Position + 1;
                    }
                    if (trackToCalculate.Begin.HasValue == false)
                    {
                        trackToCalculate.Begin = lastTrack.End;
                    }
                    else
                    {
                        if (lastTrack.End.HasValue == false)
                        {
                            lastTrack.End = trackToCalculate.Begin;
                        }
                    }
                    if (IsRecording)
                    {
                        lastTrack.End = trackToCalculate.Begin;
                    }
                }
            }
            else
            {
                if (trackToCalculate.Position.HasValue == false)
                {
                    trackToCalculate.Position = 1;
                }
                if ((trackToCalculate.Begin.HasValue == false) || (IsRecording))
                {
                    trackToCalculate.Begin = TimeSpan.Zero;
                }
            }
        }

        /// <summary>
        /// Copy values from import cuesheet to this cuesheet
        /// </summary>
        /// <param name="cuesheet">Reference to import cuesheet</param>
        /// <param name="applicationOptions">Reference to application options</param>
        private void CopyValues(ICuesheet<ImportTrack> cuesheet, ApplicationOptions applicationOptions)
        {
            Artist = cuesheet.Artist;
            Title = cuesheet.Title;
            AudioFile = cuesheet.AudioFile;
            CDTextfile = cuesheet.CDTextfile;
            CatalogueNumber = cuesheet.CatalogueNumber;
            foreach (var importTrack in cuesheet.Tracks)
            {
                var track = new Track(importTrack);
                AddTrack(track, applicationOptions);
            }
        }
    }
}
