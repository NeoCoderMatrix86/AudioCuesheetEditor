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
            set { audioFile = value; OnValidateablePropertyChanged(); }
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

        public void AddTrack(Track track)
        {
            if (track == null)
            {
                throw new ArgumentNullException(nameof(track));
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
            track.RankPropertyValueChanged += Track_RankPropertyValueChanged;
            OnValidateablePropertyChanged();
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

        public void Import(TextImportFile textImportFile)
        {
            if (textImportFile == null)
            {
                throw new ArgumentNullException(nameof(textImportFile));
            }
            if (textImportFile.IsValid == false)
            {
                throw new InvalidOperationException(String.Format("{0} was not valid!", nameof(textImportFile)));
            }
            foreach (var importTrack in textImportFile.Tracks)
            {
                var track = new Track(importTrack);
                AddTrack(track);
            }
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

        private void CatalogueNumber_ValidateablePropertyChanged(object sender, EventArgs e)
        {
            OnValidateablePropertyChanged();
        }

        private void ReCalculateTrackProperties(Track trackToCalculate)
        {
            //TODO: Get last end from audio file end
            if (Tracks.Count > 1)
            {
                var lastTrack = tracks.ElementAt(tracks.IndexOf(trackToCalculate) - 1);
                if (lastTrack != trackToCalculate)
                {
                    if (trackToCalculate.Position.HasValue == false)
                    {
                        trackToCalculate.Position = lastTrack.Position + 1;
                    }
                    if (trackToCalculate.Begin.HasValue == false)
                    {
                        trackToCalculate.Begin = lastTrack.End;
                    }
                }
            }
            else
            {
                if (trackToCalculate.Position.HasValue == false)
                {
                    trackToCalculate.Position = 1;
                }
                if (trackToCalculate.Begin.HasValue == false)
                {
                    trackToCalculate.Begin = TimeSpan.Zero;
                }
            }
        }

        public void StartRecording()
        {
            recordingStart = DateTime.UtcNow;
        }

        public void StopRecording()
        {
            recordingStart = null;
        }
    }
}
