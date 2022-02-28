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
using AudioCuesheetEditor.Model.UI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace AudioCuesheetEditor.Model.AudioCuesheet
{
    public enum MoveDirection
    {
        Up,
        Down
    }

    public class Cuesheet : Validateable, ICuesheet<Track>, ITraceable
    {
        private readonly object syncLock = new object();

        private List<Track> tracks;
        private String artist;
        private String title;
        private Audiofile audiofile;
        private CDTextfile cDTextfile;
        private Cataloguenumber catalogueNumber;
        private DateTime? recordingStart;
        private readonly List<KeyValuePair<String, Track>> currentlyHandlingLinkedTrackPropertyChange = new List<KeyValuePair<String, Track>>();
        private Stack<TraceableChange> traceableChanges;

        public event EventHandler AudioFileChanged;
        public event EventHandler<TraceablePropertiesChangedEventArgs> TraceablePropertyChanged;

        public Cuesheet()
        {
            Tracks = new List<Track>();
            Cataloguenumber = new Cataloguenumber();
            OnValidateablePropertyChanged();
        }

        [JsonInclude]
        public IReadOnlyCollection<Track> Tracks
        {
            get { return tracks.AsReadOnly(); }
            private set 
            { 
                if (value == null)
                {
                    throw new ArgumentNullException(nameof(Tracks));
                }
                tracks = value.ToList();
                foreach (var track in tracks)
                {
                    track.Cuesheet = this;
                    track.RankPropertyValueChanged += Track_RankPropertyValueChanged;
                    track.IsLinkedToPreviousTrackChanged += Track_IsLinkedToPreviousTrackChanged;
                }
            }
        }
        
        public String Artist 
        {
            get { return artist; }
            set 
            {
                var previousValue = artist;
                artist = value; 
                OnValidateablePropertyChanged();
                OnTraceablePropertyChanged(previousValue);
            }
        }
        public String Title 
        {
            get { return title; }
            set 
            {
                var previousValue = title;
                title = value; 
                OnValidateablePropertyChanged();
                OnTraceablePropertyChanged(previousValue);
            }
        }
        public Audiofile Audiofile
        {
            get { return audiofile; }
            set 
            {
                var previousValue = audiofile;
                audiofile = value; 
                OnValidateablePropertyChanged(); 
                AudioFileChanged?.Invoke(this, EventArgs.Empty);
                OnTraceablePropertyChanged(previousValue);
            }
        }

        public CDTextfile CDTextfile 
        {
            get { return cDTextfile; }
            set 
            {
                var previousValue = cDTextfile;
                cDTextfile = value; 
                OnValidateablePropertyChanged();
                OnTraceablePropertyChanged(previousValue);
            }
        }

        public Cataloguenumber Cataloguenumber 
        {
            get { return catalogueNumber; }
            set
            {
                if (catalogueNumber != null)
                {
                    catalogueNumber.ValidateablePropertyChanged -= CatalogueNumber_ValidateablePropertyChanged;
                }
                var previousValue = catalogueNumber;
                catalogueNumber = value;
                if (catalogueNumber != null)
                {
                    catalogueNumber.ValidateablePropertyChanged += CatalogueNumber_ValidateablePropertyChanged;
                }
                OnValidateablePropertyChanged();
                OnTraceablePropertyChanged(previousValue);
            }
        }

        [JsonIgnore]
        public Boolean CanWriteCuesheetFile
        {
            get
            {
                var cuesheetFile = new Cuesheetfile(this);
                return cuesheetFile.IsExportable;
            }
        }

        [JsonIgnore]
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

        /// <summary>
        /// Get the previous linked track of a track object
        /// </summary>
        /// <param name="track">Track object to get the previous link to</param>
        /// <returns>Previous linked track or null (if not linked)</returns>
        public Track GetPreviousLinkedTrack(Track track)
        {
            Track previousLinkedTrack = null;
            if (track == null)
            {
                throw new ArgumentNullException(nameof(track));
            }
            if (track.IsLinkedToPreviousTrack)
            {
                var index = tracks.IndexOf(track);
                if (index > 0)
                {
                    previousLinkedTrack = tracks.ElementAt(index - 1);
                }
            }
            return previousLinkedTrack;
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
                track.Begin = CalculateTimeSpanWithSensitivity(DateTime.UtcNow - recordingStart.Value, applicationOptions.RecordTimeSensitivity);
            }
            track.Cuesheet = this;
            var previousValue = new List<Track>(tracks);
            tracks.Add(track);
            ReCalculateTrackProperties(track);
            track.IsLinkedToPreviousTrackChanged += Track_IsLinkedToPreviousTrackChanged;
            if (applicationOptions.LinkTracksWithPreviousOne.HasValue)
            {
                track.IsLinkedToPreviousTrack = applicationOptions.LinkTracksWithPreviousOne.Value;
            }
            track.RankPropertyValueChanged += Track_RankPropertyValueChanged;
            OnValidateablePropertyChanged();
            OnTraceablePropertyChanged(previousValue, nameof(Tracks));
        }

        public void RemoveTrack(Track track)
        {
            if (track == null)
            {
                throw new ArgumentNullException(nameof(track));
            }
            var index = tracks.IndexOf(track);
            Track nextTrack = null;
            if ((index + 1) < tracks.Count)
            {
                if (tracks.ElementAt(index + 1).IsLinkedToPreviousTrack)
                {
                    nextTrack = tracks.ElementAt(index + 1);
                }
            }
            var previousValue = new List<Track>();
            tracks.ForEach(x => previousValue.Add(new Track(x)));
            tracks.Remove(track);
            track.Cuesheet = null;
            track.RankPropertyValueChanged -= Track_RankPropertyValueChanged;
            track.IsLinkedToPreviousTrackChanged -= Track_IsLinkedToPreviousTrackChanged;
            OnValidateablePropertyChanged();
            //If Tracks are linked, we need to set the linked track again
            if (nextTrack != null)
            {
                index = tracks.IndexOf(nextTrack);
                if (index > 0)
                {
                    var previousTrack = tracks.ElementAt(index - 1);
                    if (previousTrack.Position.HasValue)
                    {
                        nextTrack.Position = previousTrack.Position.Value + 1;
                    }
                    if (previousTrack.End.HasValue)
                    {
                        nextTrack.Begin = previousTrack.End.Value;
                    }
                }
            }
            OnTraceablePropertyChanged(previousValue, nameof(Tracks));
        }

        /// <summary>
        /// Remove selected tracks
        /// </summary>
        /// <param name="tracksToRemove">Selected tracks to remove (can not be null, only empty)</param>
        public void RemoveTracks(IReadOnlyCollection<Track> tracksToRemove)
        {
            if (tracksToRemove == null)
            {
                throw new ArgumentNullException(nameof(tracksToRemove));
            }
            var previousValue = new List<Track>();
            tracks.ForEach(x => previousValue.Add(new Track(x)));
            tracks.ForEach(x => x.RankPropertyValueChanged -= Track_RankPropertyValueChanged);
            tracks.ForEach(x => x.IsLinkedToPreviousTrackChanged -= Track_IsLinkedToPreviousTrackChanged);
            var intersection = tracks.Intersect(tracksToRemove);
            tracks = tracks.Except(intersection).ToList();
            foreach (var track in tracks)
            {
                if (track.IsLinkedToPreviousTrack)
                {
                    track.Position = (uint)tracks.IndexOf(track) + 1;
                }
                var previousTrack = GetPreviousLinkedTrack(track);
                if (previousTrack != null)
                {
                    if (previousTrack.End.HasValue)
                    {
                        track.Begin = previousTrack.End;
                    }
                }
            }
            tracks.ForEach(x => x.RankPropertyValueChanged += Track_RankPropertyValueChanged);
            tracks.ForEach(x => x.IsLinkedToPreviousTrackChanged += Track_IsLinkedToPreviousTrackChanged);
            OnValidateablePropertyChanged();
            OnTraceablePropertyChanged(previousValue, nameof(Tracks));
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
            Track currentTrack = null;
            switch (moveDirection)
            {
                case MoveDirection.Up:
                    if (index > 0)
                    {
                        currentTrack = tracks.ElementAt(index - 1);
                    }
                    break;
                case MoveDirection.Down:
                    if ((index + 1) < Tracks.Count)
                    {
                        currentTrack = tracks.ElementAt(index + 1);
                    }
                    break;
                default:
                    throw new ArgumentException("Invalid enum value for MoveDirection!", nameof(moveDirection));
            }
            if (currentTrack != null)
            {
                var previousValue = new List<Track>(tracks);
                SwitchTracks(track, currentTrack);
                OnTraceablePropertyChanged(previousValue, nameof(Tracks));
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
            //Since we use a stack for several changes we need to lock execution for everything else
            lock (syncLock)
            {
                traceableChanges = new Stack<TraceableChange>();
                CopyValues(textImportFile.ImportCuesheet, applicationOptions);
                TraceablePropertyChanged?.Invoke(this, new TraceablePropertiesChangedEventArgs(traceableChanges));
                traceableChanges = null;
            }
        }

        public void StartRecording()
        {
            recordingStart = DateTime.UtcNow;
        }

        public void StopRecording(ApplicationOptions applicationOptions)
        {
            //Set end of last track
            var lastTrack = Tracks.LastOrDefault();
            if (lastTrack != null)
            {
                lastTrack.End = CalculateTimeSpanWithSensitivity(DateTime.UtcNow - recordingStart.Value, applicationOptions.RecordTimeSensitivity);
            }
            recordingStart = null;
        }

        protected override void Validate()
        {
            if (String.IsNullOrEmpty(Artist) == true)
            {
                validationErrors.Add(new ValidationError(FieldReference.Create(this, nameof(Artist)), ValidationErrorType.Warning, "{0} has no value!", nameof(Artist)));
            }
            if (String.IsNullOrEmpty(Title) == true)
            {
                validationErrors.Add(new ValidationError(FieldReference.Create(this, nameof(Title)), ValidationErrorType.Warning, "{0} has no value!", nameof(Title)));
            }
            if (Audiofile == null)
            {
                validationErrors.Add(new ValidationError(FieldReference.Create(this, nameof(Audiofile)), ValidationErrorType.Error, "{0} has no value!", nameof(Audiofile)));
            }
            if (tracks.Count < 1)
            {
                validationErrors.Add(new ValidationError(FieldReference.Create(this, nameof(Tracks)), ValidationErrorType.Error, "{0} has invalid Count ({1})!", nameof(Tracks), 0));
            }
            if (CDTextfile == null)
            {
                validationErrors.Add(new ValidationError(FieldReference.Create(this, nameof(CDTextfile)), ValidationErrorType.Warning, "{0} has no value!", nameof(CDTextfile)));
            }
            if (Cataloguenumber == null)
            {
                validationErrors.Add(new ValidationError(FieldReference.Create(this, nameof(Cataloguenumber)), ValidationErrorType.Warning, "{0} has no value!", nameof(Cataloguenumber)));
            }
            else
            {
                _ = Cataloguenumber.IsValid;
                validationErrors.AddRange(Cataloguenumber.ValidationErrors);
            }
        }

        private void CatalogueNumber_ValidateablePropertyChanged(object sender, EventArgs e)
        {
            OnValidateablePropertyChanged();
        }

        private void ReCalculateTrackProperties(Track trackToCalculate)
        {
            if ((Audiofile != null) && (Audiofile.Duration.HasValue) && (trackToCalculate.End.HasValue == false))
            {
                trackToCalculate.End = Audiofile.Duration;
            }
            if (Tracks.Count > 1)
            {
                var lastTrack = tracks.ElementAt(tracks.IndexOf(trackToCalculate) - 1);
                if (lastTrack != trackToCalculate)
                {
                    if ((Audiofile != null) && (Audiofile.Duration.HasValue) && (lastTrack.End.HasValue) && (lastTrack.End.Value == Audiofile.Duration.Value))
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
            if (String.IsNullOrEmpty(cuesheet.Artist) == false)
            {
                Artist = cuesheet.Artist;
            }
            if (String.IsNullOrEmpty(cuesheet.Title) == false)
            {
                Title = cuesheet.Title;
            }
            if (cuesheet.Audiofile != null)
            {
                Audiofile = cuesheet.Audiofile;
            }
            if (cuesheet.CDTextfile != null)
            {
                CDTextfile = cuesheet.CDTextfile;
            }
            if (cuesheet.Cataloguenumber != null)
            {
                Cataloguenumber = cuesheet.Cataloguenumber;
            }
            foreach (var importTrack in cuesheet.Tracks)
            {
                var track = new Track(importTrack);
                AddTrack(track, applicationOptions);
            }
        }

        private void Track_RankPropertyValueChanged(object sender, string e)
        {
            Track trackRaisedEvent = (Track)sender;
            switch(e)
            {
                case nameof(Track.Position):
                    //Check position and call switchtracks
                    if (trackRaisedEvent.Position.HasValue)
                    {
                        var trackAtPosition = tracks.ElementAtOrDefault((int)trackRaisedEvent.Position.Value - 1);
                        if ((trackAtPosition != null) && (trackAtPosition != trackRaisedEvent))
                        {
                            SwitchTracks(trackRaisedEvent, trackAtPosition);
                        }
                    }
                    break;
            }
            var item = KeyValuePair.Create(e, trackRaisedEvent);
            if (currentlyHandlingLinkedTrackPropertyChange.Contains(item) == false)
            {
                currentlyHandlingLinkedTrackPropertyChange.Add(item);
                var linkedPreviousTrack = GetPreviousLinkedTrack(trackRaisedEvent);
                //Check if raising track has linked previous track
                if ((trackRaisedEvent.IsLinkedToPreviousTrack) && (linkedPreviousTrack != null))
                {
                    switch (e)
                    {
                        case nameof(Track.Position):
                            if (trackRaisedEvent.Position.HasValue)
                            {
                                linkedPreviousTrack.Position = trackRaisedEvent.Position.Value - 1;
                            }
                            break;
                        case nameof(Track.Begin):
                            if (trackRaisedEvent.Begin.HasValue)
                            {
                                linkedPreviousTrack.End = trackRaisedEvent.Begin;
                            }
                            break;
                    }
                }
                //Check if track is linked by next track
                var index = tracks.IndexOf(trackRaisedEvent);
                if ((index + 1) < tracks.Count)
                {
                    var nextTrack = tracks.ElementAt(index + 1);
                    if (nextTrack.IsLinkedToPreviousTrack)
                    {
                        switch (e)
                        {
                            case nameof(Track.Position):
                                if (trackRaisedEvent.Position.HasValue)
                                {
                                    nextTrack.Position = trackRaisedEvent.Position.Value + 1;
                                }
                                break;
                            case nameof(Track.End):
                                if (trackRaisedEvent.End.HasValue)
                                {
                                    nextTrack.Begin = trackRaisedEvent.End;
                                }
                                break;
                        }
                    }
                }
                currentlyHandlingLinkedTrackPropertyChange.Remove(item);
            }
        }

        private void Track_IsLinkedToPreviousTrackChanged(object sender, EventArgs e)
        {
            Track trackRaisedEvent = (Track)sender;
            if (trackRaisedEvent.IsLinkedToPreviousTrack)
            {
                //Set values
                var index = tracks.IndexOf(trackRaisedEvent);
                if (index > 0)
                {
                    var previousTrack = tracks.ElementAt(index - 1);
                    if ((trackRaisedEvent.Position.HasValue) && (trackRaisedEvent.Position != previousTrack.Position.Value + 1))
                    {
                        trackRaisedEvent.Position = previousTrack.Position.Value + 1;
                    }
                    if ((previousTrack.End.HasValue) && (trackRaisedEvent.Begin != previousTrack.End))
                    {
                        trackRaisedEvent.Begin = previousTrack.End;
                    }
                }
            }
        }

        private void OnTraceablePropertyChanged(object previousValue, [System.Runtime.CompilerServices.CallerMemberName] string propertyName = "")
        {
            if (traceableChanges == null)
            {
                var changes = new Stack<TraceableChange>();
                changes.Push(new TraceableChange(previousValue, propertyName));
                TraceablePropertyChanged?.Invoke(this, new TraceablePropertiesChangedEventArgs(changes));
            }
            else
            {
                traceableChanges.Push(new TraceableChange(previousValue, propertyName));
            }
        }

        private void SwitchTracks(Track track1, Track track2)
        {
            if (track1 == null)
            {
                throw new ArgumentNullException(nameof(track1));
            }
            if (track2 == null)
            {
                throw new ArgumentNullException(nameof(track2));
            }
            var indexTrack1 = tracks.IndexOf(track1);
            var indexTrack2 = tracks.IndexOf(track2);
            //Switch track positions in array
            tracks[indexTrack2] = track1;
            tracks[indexTrack1] = track2;
            //Set linked tracks correct again
            indexTrack1 = tracks.IndexOf(track1);
            indexTrack2 = tracks.IndexOf(track2);
            //If using linked tracks, we set values correctly
            if (track1.IsLinkedToPreviousTrack || track2.IsLinkedToPreviousTrack)
            { 
                //Set values corresponding to their new positions
                track1.Position = (uint)indexTrack1 + 1;
                track2.Position = (uint)indexTrack2 + 1;
                //Set also begin and end
                if (indexTrack1 < indexTrack2)
                {
                    track1.Begin = track2.Begin;
                    var newEnd = track1.End;
                    track1.End = track2.End;
                    track2.End = newEnd;
                    if (track2.IsLinkedToPreviousTrack)
                    {
                        var previousTrack = GetPreviousLinkedTrack(track2);
                        if (previousTrack.End.HasValue)
                        {
                            track2.Begin = previousTrack.End;
                        }
                    }
                }
                else
                {
                    track2.Begin = track1.Begin;
                    var newEnd = track2.End;
                    track2.End = track1.End;
                    track1.End = newEnd;
                    if (track1.IsLinkedToPreviousTrack)
                    {
                        var previousTrack = GetPreviousLinkedTrack(track1);
                        if (previousTrack.End.HasValue)
                        {
                            track1.Begin = previousTrack.End;
                        }
                    }
                }
            }
        }

        private TimeSpan CalculateTimeSpanWithSensitivity(TimeSpan inputTimeSpan, TimeSensitivityMode sensitivityMode)
        {
            TimeSpan timeSpan;
            switch (sensitivityMode)
            {
                default:
                case TimeSensitivityMode.Full:
                    timeSpan = inputTimeSpan;
                    break;
                case TimeSensitivityMode.Seconds:
                    timeSpan = new TimeSpan(inputTimeSpan.Days, inputTimeSpan.Hours, inputTimeSpan.Minutes, inputTimeSpan.Seconds);
                    break;
                case TimeSensitivityMode.Minutes:
                    if (inputTimeSpan.Seconds >= 30)
                    {
                        timeSpan = new TimeSpan(inputTimeSpan.Days, inputTimeSpan.Hours, inputTimeSpan.Minutes + 1, 0);
                    }
                    else
                    {
                        timeSpan = new TimeSpan(inputTimeSpan.Days, inputTimeSpan.Hours, inputTimeSpan.Minutes, 0);
                    }
                    break;
            }
            return timeSpan;
        }
    }
}
