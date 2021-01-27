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

    public class Cuesheet : Validateable, ICuesheet
    {
        private readonly object syncLock = new object();

        private readonly List<Track> tracks;
        private String artist;
        private String title;
        private AudioFile audioFile;
        private CDTextfile cDTextfile;
        private DateTime? recordingStart;

        public Cuesheet()
        {
            tracks = new List<Track>();
            CatalogueNumber = new CatalogueNumber();
            CatalogueNumber.ValidateablePropertyChanged += CatalogueNumber_ValidateablePropertyChanged;
            Validate();
        }

        public IReadOnlyCollection<Track> Tracks
        {
            get { return tracks.OrderBy(x => x.Position.HasValue == false).ThenBy(x => x.Position).ToList().AsReadOnly(); }
        }
        public uint NextFreePosition
        {
            get 
            {
                uint nextFreePosition = 1;
                if (Tracks.Count > 0)
                {
                    lock (syncLock)
                    {
                        var track = Tracks.Where(x => x.Position != null && x.Position > 0).OrderBy(x => x.Position).LastOrDefault();
                        if (track != null)
                        {
                            nextFreePosition = track.Position.Value + 1;
                        }
                    }
                }
                return nextFreePosition;
            }
        }
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
            if (IsRecording)
            {
                track.End = DateTime.UtcNow - recordingStart.Value;
            }
            tracks.Add(track);
            track.ValidateablePropertyChanged += Track_ValidateablePropertyChanged;
            ReCalculateTrackProperties();
            OnValidateablePropertyChanged();
        }

        private void Track_ValidateablePropertyChanged(object sender, EventArgs e)
        {
            OnValidateablePropertyChanged();
        }

        public void RemoveTrack(Track track)
        {
            if (track == null)
            {
                throw new ArgumentNullException(nameof(track));
            }
            tracks.Remove(track);
            track.ValidateablePropertyChanged -= Track_ValidateablePropertyChanged;
            OnValidateablePropertyChanged();
            ReCalculateTrackProperties();
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
            lock (syncLock)
            {
                var index = Tracks.ToList().IndexOf(track);
                if (moveDirection == MoveDirection.Up)
                {
                    if (index > 0)
                    {
                        var previousTrack = Tracks.ElementAt(index - 1);
                        var position = previousTrack.Position;
                        previousTrack.Position = track.Position;
                        track.Position = position;
                    }
                }
                if (moveDirection == MoveDirection.Down)
                {
                    if ((index + 1) < Tracks.Count)
                    {
                        var nextTrack = Tracks.ElementAt(index + 1);
                        var position = nextTrack.Position;
                        nextTrack.Position = track.Position;
                        track.Position = position;
                    }
                }
            }
            ReCalculateTrackProperties();
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
            //Check track overlapping
            lock (syncLock)
            {
                TimeSpan begin = TimeSpan.Zero;
                foreach (var track in Tracks)
                {
                    if ((track.Begin == null) || (track.Begin != begin))
                    {
                        validationErrors.Add(new ValidationError(FieldReference.Create(track, nameof(Track.Begin)), ValidationErrorType.Warning, "TrackHasInvalidValue", track.Position, nameof(Track.Begin), track.Begin));
                    }
                    if (track.End != null)
                    {
                        begin = track.End.Value;
                    }
                }
            }
        }

        private void CatalogueNumber_ValidateablePropertyChanged(object sender, EventArgs e)
        {
            OnValidateablePropertyChanged();
        }

        private void ReCalculateTrackProperties()
        {
            uint position = 1;
            TimeSpan? trackEnd = TimeSpan.Zero;
            Track previousTrack = null;
            lock (syncLock)
            {
                foreach (var track in Tracks)
                {
                    if (track.Position != position)
                    {
                        track.Position = position;
                    }
                    if (track.Position == 1)
                    {
                        track.Begin = TimeSpan.Zero;
                    }
                    if ((track.Begin == null) && (trackEnd != null))
                    {
                        track.Begin = trackEnd;
                    }
                    if ((track.Begin != null) && (previousTrack != null) && (previousTrack.End == null))
                    {
                        previousTrack.End = track.Begin;
                    }
                    previousTrack = track;
                    trackEnd = track.End;
                    position++;
                }
                //TODO: Get last end from audio file end
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
