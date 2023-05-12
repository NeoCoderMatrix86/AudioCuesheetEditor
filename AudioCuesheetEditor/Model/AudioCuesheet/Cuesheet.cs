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
using AudioCuesheetEditor.Model.IO;
using AudioCuesheetEditor.Model.IO.Audio;
using AudioCuesheetEditor.Model.IO.Export;
using AudioCuesheetEditor.Model.Options;
using AudioCuesheetEditor.Model.UI;
using System.Text.Json.Serialization;

namespace AudioCuesheetEditor.Model.AudioCuesheet
{
    public enum MoveDirection
    {
        Up,
        Down
    }

    public class TrackAddRemoveEventArgs : EventArgs
    {
        public TrackAddRemoveEventArgs(Track track)
        {
            Track = track;
        }

        public Track Track { get; }
    }

    public class SplitPointAddRemoveEventArgs : EventArgs
    {
        public SplitPointAddRemoveEventArgs(SplitPoint splitPoint)
        {
            SplitPoint = splitPoint;
        }

        public SplitPoint SplitPoint { get; }
    }

    public class Cuesheet : Validateable<Cuesheet>, ICuesheetEntity, ITraceable
    {
        public const String MimeType = "text/*";
        public const String FileExtension = ".cue";

        private readonly object syncLock = new();

        private List<Track> tracks;
        private String? artist;
        private String? title;
        private Audiofile? audiofile;
        private CDTextfile? cDTextfile;
        private Cataloguenumber catalogueNumber;
        private DateTime? recordingStart;
        private readonly List<KeyValuePair<String, Track>> currentlyHandlingLinkedTrackPropertyChange = new();
        private List<SplitPoint> splitPoints;

        public event EventHandler? AudioFileChanged;
        public event EventHandler<TraceablePropertiesChangedEventArgs>? TraceablePropertyChanged;
        public event EventHandler<TrackAddRemoveEventArgs>? TrackAdded;
        public event EventHandler<TrackAddRemoveEventArgs>? TrackRemoved;
        public event EventHandler<SplitPointAddRemoveEventArgs>? SplitPointAdded;
        public event EventHandler<SplitPointAddRemoveEventArgs>? SplitPointRemoved;
        public event EventHandler? CuesheetImported;

        public Cuesheet(TraceChangeManager? traceChangeManager = null)
        {
            tracks = new();
            catalogueNumber = new();
            splitPoints = new();
            TraceChangeManager = traceChangeManager;
        }

        [JsonInclude]
        public IReadOnlyCollection<Track> Tracks
        {
            get { return tracks.AsReadOnly(); }
            private set 
            {
                foreach (var track in tracks)
                {
                    track.RankPropertyValueChanged -= Track_RankPropertyValueChanged;
                    track.IsLinkedToPreviousTrackChanged -= Track_IsLinkedToPreviousTrackChanged;
                    track.Cuesheet = null;
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
        
        public String? Artist 
        {
            get => artist;
            set 
            {
                var previousValue = artist;
                artist = value;
                FireEvents(previousValue, propertyName: nameof(Artist));
            }
        }
        public String? Title 
        {
            get => title;
            set 
            {
                var previousValue = title;
                title = value;
                FireEvents(previousValue, propertyName: nameof(Title));
            }
        }
        public Audiofile? Audiofile
        {
            get => audiofile;
            set 
            {
                var previousValue = audiofile;
                if (audiofile != null)
                {
                    audiofile.ContentStreamLoaded -= Audiofile_ContentStreamLoaded;
                }
                audiofile = value;
                if (audiofile != null)
                {
                    if (audiofile.IsContentStreamLoaded == false)
                    {
                        audiofile.ContentStreamLoaded += Audiofile_ContentStreamLoaded;
                    }
                    else
                    {
                        RecalculateLastTrackEnd();
                    }
                }
                FireEvents(previousValue, fireAudioFileChanged: true, propertyName: nameof(Audiofile));
            }
        }

        public CDTextfile? CDTextfile 
        {
            get => cDTextfile;
            set 
            {
                var previousValue = cDTextfile;
                cDTextfile = value;
                FireEvents(previousValue, fireValidateablePropertyChanged: false, propertyName: nameof(CDTextfile));
            }
        }

        public Cataloguenumber Cataloguenumber 
        {
            get => catalogueNumber;
            set
            {
                var previousValue = catalogueNumber;
                catalogueNumber = value;
                FireEvents(previousValue, fireValidateablePropertyChanged: false, propertyName: nameof(Cataloguenumber));
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

        [JsonIgnore]
        public Boolean IsImporting { get; private set; }
        
        [JsonInclude]
        public IReadOnlyCollection<SplitPoint> SplitPoints 
        {
            get => splitPoints;
            private set
            {
                foreach(var splitpoint in value.Where(x => x.Cuesheet != this))
                {
                    splitpoint.Cuesheet = this;
                }
                splitPoints = value.ToList();
            }
        }

        [JsonIgnore]
        public TraceChangeManager? TraceChangeManager { get; }

        public SplitPoint AddSplitPoint()
        {
            var previousValue = new List<SplitPoint>(splitPoints);
            var splitPoint = new SplitPoint(this);
            splitPoints.Add(splitPoint);
            SplitPointAdded?.Invoke(this, new SplitPointAddRemoveEventArgs(splitPoint));
            OnTraceablePropertyChanged(previousValue, nameof(SplitPoints));
            return splitPoint;
        }

        public void RemoveSplitPoint(SplitPoint splitPoint)
        {
            var previousValue = new List<SplitPoint>(splitPoints);
            if (splitPoints.Remove(splitPoint))
            {
                OnTraceablePropertyChanged(previousValue, nameof(SplitPoints));
                SplitPointRemoved?.Invoke(this, new SplitPointAddRemoveEventArgs(splitPoint));
            }
        }

        public SplitPoint? GetSplitPointAtTrack(Track track)
        {
            SplitPoint? splitPointAtTrack = SplitPoints?.FirstOrDefault(x => track.Begin < x.Moment && track.End >= x.Moment);
            return splitPointAtTrack;
        }

        /// <summary>
        /// Get the previous linked track of a track object
        /// </summary>
        /// <param name="track">Track object to get the previous link to</param>
        /// <returns>Previous linked track or null (if not linked)</returns>
        public Track? GetPreviousLinkedTrack(Track track)
        {
            Track? previousLinkedTrack = null;
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

        public void AddTrack(Track track, ApplicationOptions? applicationOptions = null)
        {
            if (track.IsCloned)
            {
                throw new ArgumentException("Cloned tracks may not be added!");
            }
            var previousValue = new List<Track>(tracks);
            track.IsLinkedToPreviousTrackChanged += Track_IsLinkedToPreviousTrackChanged;
            //When no applications are available (because of used by import for example) we don't try to calculate properties
            if (applicationOptions != null)
            {
                if (IsRecording && (recordingStart.HasValue))
                {
                    track.Begin = CalculateTimeSpanWithSensitivity(DateTime.UtcNow - recordingStart.Value, applicationOptions.RecordTimeSensitivity);
                }                
                track.IsLinkedToPreviousTrack = applicationOptions.LinkTracksWithPreviousOne;
            }
            tracks.Add(track);
            track.Cuesheet = this;
            ReCalculateTrackProperties(track);
            track.RankPropertyValueChanged += Track_RankPropertyValueChanged;
            OnTraceablePropertyChanged(previousValue, nameof(Tracks));
            if (IsImporting == false)
            {
                TrackAdded?.Invoke(this, new TrackAddRemoveEventArgs(track));
            }
        }

        public void RemoveTrack(Track track)
        {
            var index = tracks.IndexOf(track);
            Track? nextTrack = null;
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
            RecalculateLastTrackEnd();
            OnTraceablePropertyChanged(previousValue, nameof(Tracks));
            TrackRemoved?.Invoke(this, new TrackAddRemoveEventArgs(track));
        }

        /// <summary>
        /// Remove selected tracks
        /// </summary>
        /// <param name="tracksToRemove">Selected tracks to remove (can not be null, only empty)</param>
        public void RemoveTracks(IReadOnlyCollection<Track> tracksToRemove)
        {
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
            RecalculateLastTrackEnd();
            OnTraceablePropertyChanged(previousValue, nameof(Tracks));
        }

        public Boolean MoveTrackPossible(Track track, MoveDirection moveDirection)
        {
            Boolean movePossible = false;
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
            var index = tracks.IndexOf(track);
            Track? currentTrack = null;
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

        public void Import(Cuesheet cuesheet, ApplicationOptions applicationOptions, TraceChangeManager? traceChangeManager = null)
        {
            //Since we use a stack for several changes we need to lock execution for everything else
            lock (syncLock)
            {
                IsImporting = true;
                //We are doing a bulk edit, so inform the TraceChangeManager
                if (traceChangeManager != null)
                {
                    traceChangeManager.BulkEdit = true;
                }
                CopyValues(cuesheet, applicationOptions);
                if (traceChangeManager != null)
                {
                    traceChangeManager.BulkEdit = false;
                }
                IsImporting = false;
            }
            CuesheetImported?.Invoke(this, EventArgs.Empty);
        }

        public void StartRecording()
        {
            recordingStart = DateTime.UtcNow;
        }

        public void StopRecording(ApplicationOptions applicationOptions)
        {
            //Set end of last track
            var lastTrack = Tracks.LastOrDefault();
            if ((lastTrack != null) && (recordingStart.HasValue))
            {
                lastTrack.End = CalculateTimeSpanWithSensitivity(DateTime.UtcNow - recordingStart.Value, applicationOptions.RecordTimeSensitivity);
            }
            recordingStart = null;
        }

        protected override ValidationResult Validate(string property)
        {
            ValidationStatus validationStatus = ValidationStatus.NoValidation;
            List<ValidationMessage>? validationMessages = null;
            switch (property)
            {
                case nameof(Tracks):
                    validationStatus = ValidationStatus.Success;
                    if (Tracks.Count <= 0)
                    {
                        validationMessages ??= new();
                        validationMessages.Add(new ValidationMessage("{0} has invalid Count ({1})!", nameof(Tracks), 0));
                    }
                    else
                    {
                        //Check track overlapping
                        var tracksWithSamePosition = Tracks.Where(x => x.IsCloned == false)
                            .GroupBy(x => x.Position)
                            .Where(grp => grp.Count() > 1);
                        if (tracksWithSamePosition.Any())
                        {
                            validationMessages ??= new();
                            foreach (var track in tracksWithSamePosition)
                            {
                                foreach (var trackWithSamePosition in track)
                                {
                                    validationMessages.Add(new ValidationMessage("{0} {1} '{2}' is used also by {3}({4},{5},{6},{7},{8}). Positions must be unique!", nameof(Track), nameof(Track.Position), track.Key != null ? track.Key : String.Empty, nameof(Track), trackWithSamePosition.Position != null ? trackWithSamePosition.Position : String.Empty, trackWithSamePosition.Artist ?? String.Empty, trackWithSamePosition.Title ?? String.Empty, trackWithSamePosition.Begin != null ? trackWithSamePosition.Begin : String.Empty, trackWithSamePosition.End != null ? trackWithSamePosition.End : String.Empty));
                                }
                            }
                        }
                        foreach (var track in Tracks.OrderBy(x => x.Position))
                        {
                            var tracksBetween = Tracks.Where(x => ((track.Begin >= x.Begin && track.Begin < x.End)
                                                        || (x.Begin < track.End && track.End <= x.End))
                                                        && (x.Equals(track) == false));
                            if (tracksBetween.Any())
                            {
                                validationMessages ??= new();
                                foreach (var trackBetween in tracksBetween)
                                {
                                    validationMessages.Add(new ValidationMessage("{0}({1},{2},{3},{4},{5}) is overlapping with {0}({6},{7},{8},{9},{10}). Please make shure the timeinterval is only used once!", nameof(Track), track.Position != null ? track.Position : String.Empty, track.Artist ?? String.Empty, track.Title ?? String.Empty, track.Begin != null ? track.Begin : String.Empty, track.End != null ? track.End : String.Empty, trackBetween.Position != null ? trackBetween.Position : String.Empty, trackBetween.Artist ?? String.Empty, trackBetween.Title ?? String.Empty, trackBetween.Begin != null ? trackBetween.Begin : String.Empty, trackBetween.End != null ? trackBetween.End : String.Empty));
                                }
                            }
                        }
                    }
                    break;
                case nameof(Audiofile):
                    validationStatus = ValidationStatus.Success;
                    if (Audiofile == null)
                    {
                        validationMessages ??= new();
                        validationMessages.Add(new ValidationMessage("{0} has no value!", nameof(Audiofile)));
                    }
                    break;
                case nameof(Artist):
                    validationStatus = ValidationStatus.Success;
                    if (String.IsNullOrEmpty(Artist))
                    {
                        validationMessages ??= new();
                        validationMessages.Add(new ValidationMessage("{0} has no value!", nameof(Artist)));
                    }
                    break;
                case nameof(Title):
                    validationStatus = ValidationStatus.Success;
                    if (String.IsNullOrEmpty(Title))
                    {
                        validationMessages ??= new();
                        validationMessages.Add(new ValidationMessage("{0} has no value!", nameof(Title)));
                    }
                    break;
            }
            return ValidationResult.Create(validationStatus, validationMessages);
        }

        private void ReCalculateTrackProperties(Track trackToCalculate)
        {
            if ((Audiofile != null) && (Audiofile.Duration.HasValue) && (trackToCalculate.End.HasValue == false))
            {
                trackToCalculate.End = Audiofile.Duration;
                TraceChangeManager?.MergeLastEditWithEdit(x => x.Changes.All(y => y.TraceableObject == this && y.TraceableChange.PropertyName == nameof(Audiofile)));
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
        private void CopyValues(Cuesheet cuesheet, ApplicationOptions applicationOptions)
        {
            Artist = cuesheet.Artist;
            Title = cuesheet.Title;
            Audiofile = cuesheet.Audiofile;
            CDTextfile = cuesheet.CDTextfile;
            Cataloguenumber = cuesheet.Cataloguenumber;
            foreach (var importTrack in cuesheet.Tracks)
            {
                //We don't want to copy the cuesheet reference since we are doing a copy and want to assign the track to this object
                var track = new Track(importTrack, false);
                AddTrack(track, applicationOptions);
            }
            // Copy splitpoints
            foreach (var splitPoint in cuesheet.SplitPoints)
            {
                var newSplitPoint = AddSplitPoint();
                newSplitPoint.CopyValues(splitPoint);
            }
        }

        private void Track_RankPropertyValueChanged(object? sender, string e)
        {
            if (sender is Track trackRaisedEvent)
            {
                var item = KeyValuePair.Create(e, trackRaisedEvent);
                if (currentlyHandlingLinkedTrackPropertyChange.Contains(item) == false)
                {
                    currentlyHandlingLinkedTrackPropertyChange.Add(item);
                    var linkedPreviousTrack = GetPreviousLinkedTrack(trackRaisedEvent);
                    //Check if raising track has linked previous track
                    if (trackRaisedEvent.IsLinkedToPreviousTrack && (linkedPreviousTrack != null))
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
                                    nextTrack.Begin = trackRaisedEvent.End;
                                    break;
                            }
                        }
                    }
                    currentlyHandlingLinkedTrackPropertyChange.Remove(item);
                }
            }
            else
            {
                throw new ArgumentNullException(nameof(sender));
            }
        }

        private void Track_IsLinkedToPreviousTrackChanged(object? sender, EventArgs e)
        {
            if (sender != null)
            {
                Track trackRaisedEvent = (Track)sender;
                if (trackRaisedEvent.IsLinkedToPreviousTrack)
                {
                    //Set values
                    var index = tracks.IndexOf(trackRaisedEvent);
                    if (index > 0)
                    {
                        var previousTrack = tracks.ElementAt(index - 1);
                        if ((trackRaisedEvent.Position.HasValue) && (previousTrack.Position.HasValue) && (trackRaisedEvent.Position != previousTrack.Position.Value + 1))
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
            else
            {
                throw new ArgumentNullException(nameof(sender));
            }
        }

        private void OnTraceablePropertyChanged(object? previousValue, [System.Runtime.CompilerServices.CallerMemberName] string propertyName = "")
        {
            TraceablePropertyChanged?.Invoke(this, new TraceablePropertiesChangedEventArgs(new TraceableChange(previousValue, propertyName)));
        }

        private void Audiofile_ContentStreamLoaded(object? sender, EventArgs e)
        {
            RecalculateLastTrackEnd();
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
                    (track2.End, track1.End) = (track1.End, track2.End);
                    if (track2.IsLinkedToPreviousTrack)
                    {
                        var previousTrack = GetPreviousLinkedTrack(track2);
                        if ((previousTrack != null) && (previousTrack.End.HasValue))
                        {
                            track2.Begin = previousTrack.End;
                        }
                    }
                }
                else
                {
                    track2.Begin = track1.Begin;
                    (track1.End, track2.End) = (track2.End, track1.End);
                    if (track1.IsLinkedToPreviousTrack)
                    {
                        var previousTrack = GetPreviousLinkedTrack(track1);
                        if ((previousTrack != null) && (previousTrack.End.HasValue))
                        {
                            track1.Begin = previousTrack.End;
                        }
                    }
                }
            }
        }

        private static TimeSpan CalculateTimeSpanWithSensitivity(TimeSpan inputTimeSpan, TimeSensitivityMode sensitivityMode)
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

        private void RecalculateLastTrackEnd()
        {
            //Try to recalculate length by recalculating last track
            var lastTrack = tracks.LastOrDefault();
            if (lastTrack != null)
            {
                ReCalculateTrackProperties(lastTrack);
            }
        }

        /// <summary>
        /// Method for checking if fire of events should be done
        /// </summary>
        /// <param name="previousValue">Previous value of the property firing events</param>
        /// <param name="fireAudioFileChanged">Fire AudioFileChanged?</param>
        /// <param name="fireValidateablePropertyChanged">Fire OnValidateablePropertyChanged?</param>
        /// <param name="fireTraceablePropertyChanged">Fire TraceablePropertyChanged?</param>
        /// <param name="propertyName">Property firing the event</param>
        /// <exception cref="NullReferenceException">If propertyName can not be found, an exception is thrown.</exception>
        private void FireEvents(object? previousValue, Boolean fireAudioFileChanged = false, Boolean fireValidateablePropertyChanged = true, Boolean fireTraceablePropertyChanged = true, [System.Runtime.CompilerServices.CallerMemberName] string propertyName = "")
        {
            var propertyInfo = GetType().GetProperty(propertyName);
            if (propertyInfo != null)
            {
                var propertyValue = propertyInfo.GetValue(this);
                if (Equals(propertyValue, previousValue) == false)
                {
                    if (fireAudioFileChanged)
                    {
                        AudioFileChanged?.Invoke(this, EventArgs.Empty);
                    }
                    if (fireValidateablePropertyChanged)
                    {
                        OnValidateablePropertyChanged(propertyName);
                    }
                    if (fireTraceablePropertyChanged)
                    {
                        OnTraceablePropertyChanged(previousValue, propertyName);
                    }
                }
            }
            else
            {
                throw new NullReferenceException(String.Format("Property {0} could not be found!", propertyName));
            }
        }
    }
}
