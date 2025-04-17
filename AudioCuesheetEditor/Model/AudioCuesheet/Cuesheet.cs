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
using AudioCuesheetEditor.Model.IO.Export;
using AudioCuesheetEditor.Model.UI;
using System.Text.Json.Serialization;

namespace AudioCuesheetEditor.Model.AudioCuesheet
{
    public enum MoveDirection
    {
        Up,
        Down
    }

    public class TracksAddedRemovedEventArgs(IEnumerable<Track> tracks) : EventArgs
    {
        public IEnumerable<Track> Tracks { get; } = tracks;
    }

    public class Cuesheet() : Validateable, ITraceable, ICuesheet
    {
        private readonly Lock syncLock = new();

        private List<Track> tracks = [];
        private String? artist;
        private String? title;
        private IAudiofile? audiofile;
        private CDTextfile? cDTextfile;
        private String? catalogueNumber;
        private DateTime? recordingStart;
        private readonly List<KeyValuePair<String, Track>> currentlyHandlingLinkedTrackPropertyChange = [];
        private List<CuesheetSection> sections = [];

        public event EventHandler<TraceablePropertiesChangedEventArgs>? TraceablePropertyChanged;
        public event EventHandler? IsRecordingChanged;

        [JsonInclude]
        public IReadOnlyCollection<Track> Tracks
        {
            get => tracks.AsReadOnly();
            private set
            {
                foreach (var track in tracks)
                {
                    track.RankPropertyValueChanged -= Track_RankPropertyValueChanged;
                    track.IsLinkedToPreviousTrackChanged -= Track_IsLinkedToPreviousTrackChanged;
                    track.Cuesheet = null;
                }
                tracks = [.. value];
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
        public IAudiofile? Audiofile
        {
            get => audiofile;
            set 
            {
                var previousValue = audiofile;
                audiofile = value;
                FireEvents(previousValue, propertyName: nameof(Audiofile));
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

        public String? Cataloguenumber 
        {
            get => catalogueNumber;
            set
            {
                var previousValue = catalogueNumber;
                catalogueNumber = value;
                FireEvents(previousValue, propertyName: nameof(Cataloguenumber));
            }
        }

        [JsonIgnore]
        public bool IsRecording => RecordingTime.HasValue;

        //TODO: Remove this property and make recordingStart public
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

        public IEnumerable<String> IsRecordingPossible
        {
            get
            {
                var errors = new List<String>();
                if (IsRecording)
                {
                    errors.Add("Record is already running!");
                }
                if (Tracks.Count != 0)
                {
                    errors.Add("Cuesheet already contains tracks!");
                }
                if (Audiofile?.IsRecorded == true)
                {
                    errors.Add("A recording is already available!");
                }
                return errors;
            }
        }

        [JsonIgnore]
        public Boolean IsImporting { get; set; }
        
        [JsonInclude]
        public IReadOnlyCollection<CuesheetSection> Sections 
        {
            get => sections;
            private set
            {
                foreach(var section in value.Where(x => x.Cuesheet != this))
                {
                    section.Cuesheet = this;
                }
                sections = [.. value];
            }
        }

        public CuesheetSection AddSection()
        {
            var previousValue = new List<CuesheetSection>(sections);
            var section = new CuesheetSection(this);
            sections.Add(section);
            OnTraceablePropertyChanged(previousValue, nameof(Sections));
            return section;
        }

        public void RemoveSections(IEnumerable<CuesheetSection> sectionsToRemove)
        {
            var previousValue = new List<CuesheetSection>(sections);
            var intersection = sections.Intersect(sectionsToRemove);
            sections = [.. sections.Except(intersection)];
            OnTraceablePropertyChanged(previousValue, nameof(Sections));
        }

        public CuesheetSection? GetSection(Track track)
        {
            return Sections?.FirstOrDefault(x => track.Begin <= x.Begin && track.End >= x.Begin);
        }

        /// <summary>
        /// Get the previous linked track of a track object
        /// </summary>
        /// <param name="track">Track object to get the previous link to</param>
        /// <returns>Previous linked track or null (if not linked)</returns>
        public Track? GetPreviousLinkedTrack(Track track)
        {
            Track? previousLinkedTrack = null;
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

        public void AddTrack(Track track)
        {
            if (track.IsCloned)
            {
                throw new ArgumentException("Cloned tracks may not be added!");
            }
            var previousValue = new List<Track>(tracks);
            track.IsLinkedToPreviousTrackChanged += Track_IsLinkedToPreviousTrackChanged;
            if (IsRecording && recordingStart.HasValue)
            {
                track.Begin = DateTime.UtcNow - recordingStart.Value;
            }
            //Fire the event manually since we don't know if the track is already linked to previous one
            Track_IsLinkedToPreviousTrackChanged(track, EventArgs.Empty);
            tracks.Add(track);
            track.Cuesheet = this;
            RecalculateTrackProperties(track);
            track.RankPropertyValueChanged += Track_RankPropertyValueChanged;
            OnTraceablePropertyChanged(previousValue, nameof(Tracks));
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
            tracks = [.. tracks.Except(intersection)];
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
        public Boolean MoveTracksPossible(IEnumerable<Track> tracksToMove, MoveDirection moveDirection)
        {
            lock (syncLock)
            {
                var trackIndices = tracksToMove.Select(t => tracks.IndexOf(t)).Where(i => i >= 0).OrderBy(i => i).ToList();

                if (trackIndices.Count == 0)
                {
                    return false;
                }

                if (moveDirection == MoveDirection.Up)
                {
                    return trackIndices.First() > 0;
                }
                if (moveDirection == MoveDirection.Down)
                {
                    return trackIndices.Last() < Tracks.Count - 1;
                }

                return false;
            }
        }
        public void MoveTracks(IEnumerable<Track> tracksToMove, MoveDirection moveDirection)
        {
            lock (syncLock)
            {
                if (!MoveTracksPossible(tracksToMove, moveDirection))
                {
                    return;
                }
                var trackIndices = tracksToMove.Select(t => tracks.IndexOf(t)).Where(i => i >= 0).OrderBy(i => i).ToList();

                var previousValue = new List<Track>(Tracks);

                if (moveDirection == MoveDirection.Up)
                {
                    foreach (var index in trackIndices)
                    {
                        if (index > 0)
                        {
                            SwitchTracks(tracks[index], tracks[index - 1]);
                        }
                    }
                }
                else if (moveDirection == MoveDirection.Down)
                {
                    for (int i = trackIndices.Count - 1; i >= 0; i--)
                    {
                        int index = trackIndices[i];
                        if (index < Tracks.Count - 1)
                        {
                            SwitchTracks(tracks[index], tracks[index + 1]);
                        }
                    }
                }

                OnTraceablePropertyChanged(previousValue, nameof(Tracks));
            }
        }

        public void StartRecording()
        {
            if (IsRecordingPossible.Any() == false)
            {
                recordingStart = DateTime.UtcNow;
                IsRecordingChanged?.Invoke(this, EventArgs.Empty);
            }
        }

        public void StopRecording()
        {
            //Set end of last track
            var lastTrack = Tracks.LastOrDefault();
            if ((lastTrack != null) && (recordingStart.HasValue))
            {
                lastTrack.End = DateTime.UtcNow - recordingStart.Value;
            }
            recordingStart = null;
            IsRecordingChanged?.Invoke(this, EventArgs.Empty);
        }

        public void RecalculateLastTrackEnd()
        {
            //Try to recalculate length by recalculating last track
            var lastTrack = tracks.LastOrDefault();
            if (lastTrack != null)
            {
                RecalculateTrackProperties(lastTrack);
            }
        }

        public override ValidationResult Validate(string property)
        {
            ValidationStatus validationStatus = ValidationStatus.NoValidation;
            List<ValidationMessage>? validationMessages = null;
            switch (property)
            {
                case nameof(Tracks):
                    validationStatus = ValidationStatus.Success;
                    if (Tracks.Count <= 0)
                    {
                        validationMessages ??= [];
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
                            validationMessages ??= [];
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
                                validationMessages ??= [];
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
                        validationMessages ??= [];
                        validationMessages.Add(new ValidationMessage("{0} has no value!", nameof(Audiofile)));
                    }
                    break;
                case nameof(Artist):
                    validationStatus = ValidationStatus.Success;
                    if (String.IsNullOrEmpty(Artist))
                    {
                        validationMessages ??= [];
                        validationMessages.Add(new ValidationMessage("{0} has no value!", nameof(Artist)));
                    }
                    break;
                case nameof(Title):
                    validationStatus = ValidationStatus.Success;
                    if (String.IsNullOrEmpty(Title))
                    {
                        validationMessages ??= [];
                        validationMessages.Add(new ValidationMessage("{0} has no value!", nameof(Title)));
                    }
                    break;
                case nameof(Cataloguenumber):
                    validationStatus = ValidationStatus.Success;
                    if (String.IsNullOrEmpty(Cataloguenumber) == false)
                    {
                        if (Cataloguenumber.All(Char.IsDigit) == false)
                        {
                            validationMessages ??= [];
                            validationMessages.Add(new ValidationMessage("{0} must only contain numbers!", nameof(Cataloguenumber)));
                        }
                        if (Cataloguenumber.Length != 13)
                        {
                            validationMessages ??= [];
                            validationMessages.Add(new ValidationMessage("{0} has an invalid length. Allowed length is {1}!", nameof(Cataloguenumber), 13));
                        }
                    }
                    break;
            }
            return ValidationResult.Create(validationStatus, validationMessages);
        }

        private void RecalculateTrackProperties(Track trackToCalculate)
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
                        if ((previousTrack.End.HasValue == false) && (trackRaisedEvent.Begin.HasValue))
                        {
                            previousTrack.End = trackRaisedEvent.Begin;
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

        private void SwitchTracks(Track track1, Track track2)
        {
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

        /// <summary>
        /// Method for checking if fire of events should be done
        /// </summary>
        /// <param name="previousValue">Previous value of the property firing events</param>
        /// <param name="fireValidateablePropertyChanged">Fire OnValidateablePropertyChanged?</param>
        /// <param name="fireTraceablePropertyChanged">Fire TraceablePropertyChanged?</param>
        /// <param name="propertyName">Property firing the event</param>
        /// <exception cref="NullReferenceException">If propertyName can not be found, an exception is thrown.</exception>
        private void FireEvents(object? previousValue, Boolean fireValidateablePropertyChanged = true, Boolean fireTraceablePropertyChanged = true, [System.Runtime.CompilerServices.CallerMemberName] string propertyName = "")
        {
            var propertyInfo = GetType().GetProperty(propertyName);
            if (propertyInfo != null)
            {
                var propertyValue = propertyInfo.GetValue(this);
                if (Equals(propertyValue, previousValue) == false)
                {
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
