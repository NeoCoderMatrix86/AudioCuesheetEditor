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
using AudioCuesheetEditor.Model.Reflection;
using AudioCuesheetEditor.Model.UI;
using Blazorise.Localization;
using System.Text.Json.Serialization;

namespace AudioCuesheetEditor.Model.AudioCuesheet
{
    public enum SetFlagMode
    {
        Add,
        Remove
    }

    public class Track : Validateable, ICuesheetEntity, ITraceable
    {
        public static readonly List<String> AllPropertyNames = new() { nameof(IsLinkedToPreviousTrack), nameof(Position), nameof(Artist), nameof(Title), nameof(Begin), nameof(End), nameof(Flags), nameof(PreGap), nameof(PostGap), nameof(Length) };

        private uint? position;
        private String? artist;
        private String? title;
        private TimeSpan? begin;
        private TimeSpan? end;
        private TimeSpan? _length;
        private List<Flag> flags = new();
        private Track? clonedFrom = null;
        private Boolean isLinkedToPreviousTrack;
        private Cuesheet? cuesheet;
        private TimeSpan? preGap;
        private TimeSpan? postGap;

        /// <summary>
        /// A property with influence to position of this track in cuesheet has been changed. Name of the property changed is provided in event arguments.
        /// </summary>
        public event EventHandler<String>? RankPropertyValueChanged;

        /// <summary>
        /// Eventhandler for IsLinkedToPreviousTrack has changed
        /// </summary>
        public event EventHandler? IsLinkedToPreviousTrackChanged;

        /// <inheritdoc/>
        public event EventHandler<TraceablePropertiesChangedEventArgs>? TraceablePropertyChanged;

        /// <summary>
        /// Create object with copied values from input
        /// </summary>
        /// <param name="track">Object to copy values from</param>
        /// /// <param name="copyCuesheetReference">Copy cuesheet reference from track also?</param>
        public Track(Track track, Boolean copyCuesheetReference = true)
        {
            CopyValues(track, copyCuesheetReference);
        }

        public Track()
        {
            Validate();
        }
        public uint? Position 
        {
            get => position;
            set { var previousValue = position; position = value; FireEvents(previousValue, propertyName: nameof(Position)); }
        }
        public String? Artist 
        {
            get => artist;
            set { var previousValue = artist; artist = value; FireEvents(previousValue, fireRankPropertyValueChanged: false, propertyName: nameof(Artist)); }
        }
        public String? Title 
        {
            get => title;
            set { var previousValue = title; title = value; FireEvents(previousValue, fireRankPropertyValueChanged: false, propertyName: nameof(Title)); }
        }
        public TimeSpan? Begin 
        {
            get => begin;
            set { var previousValue = begin; begin = value; FireEvents(previousValue, propertyName: nameof(Begin)); }
        }
        public TimeSpan? End 
        {
            get => end;
            set { var previousValue = end; end = value; FireEvents(previousValue, propertyName: nameof(End)); }
        }
        /// <summary>
        /// If <see cref="Length"/> is set, should it be automatically change begin and end? Defaulting to true, because only during edit dialog this should be set to false. 
        /// If set to false an internal field will be used.
        /// </summary>
        [JsonIgnore]
        public Boolean AutomaticallyCalculateLength { get; init; } = true;
        [JsonIgnore]
        public TimeSpan? Length 
        {
            get
            {
                if (AutomaticallyCalculateLength)
                {
                    if ((Begin.HasValue == true) && (End.HasValue == true) && (Begin.Value <= End.Value))
                    {
                        return End - Begin;
                    }
                    else
                    {
                        return null;
                    }
                }
                else
                {
                    return _length;
                }
            }
            set
            {
                if (AutomaticallyCalculateLength)
                {
                    if ((Begin.HasValue == false) && (End.HasValue == false))
                    {
                        Begin = TimeSpan.Zero;
                        End = Begin.Value + value;
                    }
                    else
                    {
                        if ((Begin.HasValue == true) && (End.HasValue == true))
                        {
                            End = Begin.Value + value;
                        }
                        else
                        {
                            if ((End.HasValue == false) && (Begin.HasValue))
                            {
                                End = Begin.Value + value;
                            }
                            if ((Begin.HasValue == false) && (End.HasValue))
                            {
                                Begin = End.Value - value;
                            }
                        }
                    }
                }
                else
                {
                    _length = value;
                }
                OnValidateablePropertyChanged();
            }
        }
        [JsonInclude]
        public IReadOnlyCollection<Flag> Flags
        {
            get { return flags.AsReadOnly(); }
            private set
            {
                if (value == null)
                {
                    throw new ArgumentNullException(nameof(Flags));
                }
                flags = value.ToList();
            }
        }
        [JsonIgnore]
        public Cuesheet? Cuesheet 
        {
            get => cuesheet;
            set { var previousValue = cuesheet; cuesheet = value; FireEvents(previousValue, fireRankPropertyValueChanged:false, propertyName: nameof(Cuesheet)); }
        }

        /// <summary>
        /// Indicates that this track has been cloned from another track and is a transparent proxy
        /// </summary>
        [JsonIgnore]
        public Boolean IsCloned { get { return clonedFrom != null; } }
        /// <summary>
        /// Get the original track that this track has been cloned from. Can be null on original objects
        /// </summary>
        public Track? ClonedFrom
        {
            get => clonedFrom;
            private set { clonedFrom = value; OnValidateablePropertyChanged(); }
        }
        /// <inheritdoc/>
        public TimeSpan? PreGap 
        {
            get => preGap;
            set { var previousValue = preGap; preGap = value; FireEvents(previousValue, fireValidateablePropertyChanged: false, fireRankPropertyValueChanged: false, propertyName: nameof(PreGap)); }
        }
        /// <inheritdoc/>
        public TimeSpan? PostGap 
        {
            get => postGap;
            set { var previousValue = postGap; postGap = value; FireEvents(previousValue, fireValidateablePropertyChanged: false, fireRankPropertyValueChanged: false, propertyName: nameof(PostGap)); }
        }
        /// <summary>
        /// Set that this track is linked to the previous track in cuesheet
        /// </summary>
        public Boolean IsLinkedToPreviousTrack
        {
            get => isLinkedToPreviousTrack;
            set { var previousValue = IsLinkedToPreviousTrack; isLinkedToPreviousTrack = value; IsLinkedToPreviousTrackChanged?.Invoke(this, EventArgs.Empty); FireEvents(previousValue, fireValidateablePropertyChanged: false, fireRankPropertyValueChanged: false, propertyName: nameof(IsLinkedToPreviousTrack)); }
        }

        public String? GetDisplayNameLocalized(ITextLocalizer localizer)
        {
            String? identifierString = null;
            if (Position != null)
            {
                identifierString += String.Format("{0}", Position);
            }
            if (identifierString == null)
            {
                if (String.IsNullOrEmpty(Artist) == false)
                {
                    identifierString += String.Format("{0}", Artist);
                }
                if (String.IsNullOrEmpty(Title) == false)
                {
                    if (identifierString != null)
                    {
                        identifierString += ",";
                    }
                    identifierString += String.Format("{0}", Title);
                }
            }
            return String.Format("{0} ({1})", localizer[nameof(Track)], identifierString);
        }

        /// <summary>
        /// Performs a deep clone of the current object
        /// </summary>
        /// <returns>A deep clone of the current object</returns>
        public Track Clone()
        {
            return new Track(this)
            {
                ClonedFrom = this
            };
        }

        /// <summary>
        /// Copy values from input object to this object
        /// </summary>
        /// <param name="track">Object to copy values from</param>
        /// <param name="setCuesheet">Copy Cuesheet reference from track also?</param>
        /// <param name="setIsLinkedToPreviousTrack">Copy IsLinkedToPreviousTrack from track also?</param>
        /// <param name="setPosition">Copy Position from track also?</param>
        /// <param name="setArtist">Copy Artist from track also?</param>
        /// <param name="setTitle">Copy Title from track also?</param>
        /// <param name="setBegin">Copy Begin from track also?</param>
        /// <param name="setEnd">Copy End from track also?</param>
        /// <param name="setLength">Copy Length from track also?</param>
        /// <param name="setFlags">Copy Flags from track also?</param>
        /// <param name="setPreGap">Copy PreGap from track also?</param>
        /// <param name="setPostGap">Copy PostGap from track also?</param>
        /// <param name="useInternalSetters">A list of properties, for whom the internal set construct should be used, in order to avoid automatic calculation.</param>
        public void CopyValues(Track track, Boolean setCuesheet = true, Boolean setIsLinkedToPreviousTrack = true, Boolean setPosition = true, Boolean setArtist = true, Boolean setTitle = true, Boolean setBegin = true, Boolean setEnd = true, Boolean setLength = false, Boolean setFlags = true, Boolean setPreGap = true, Boolean setPostGap = true, IEnumerable<String>? useInternalSetters = null)
        {
            if (setCuesheet)
            {
                if ((useInternalSetters != null) && (useInternalSetters.Contains(nameof(Cuesheet))))
                {
                    cuesheet = track.Cuesheet;
                }
                else
                {
                    Cuesheet = track.Cuesheet;
                }
            }
            if (setPosition)
            {
                if ((useInternalSetters != null) && (useInternalSetters.Contains(nameof(Position))))
                {
                    position = track.Position;
                }
                else
                {
                    Position = track.Position;
                }
            }
            if (setArtist)
            {
                if ((useInternalSetters != null) && (useInternalSetters.Contains(nameof(Artist))))
                {
                    artist = track.Artist;
                }
                else
                {
                    Artist = track.Artist;
                }
            }
            if (setTitle)
            {
                if ((useInternalSetters != null) && (useInternalSetters.Contains(nameof(Title))))
                {
                    title = track.Title;
                }
                else
                {
                    Title = track.Title;
                }
            }
            if (setBegin)
            {
                if ((useInternalSetters != null) && (useInternalSetters.Contains(nameof(Begin))))
                {
                    begin = track.Begin;
                }
                else
                {
                    Begin = track.Begin;
                }
            }
            if (setEnd)
            {
                if ((useInternalSetters != null) && (useInternalSetters.Contains(nameof(End))))
                {
                    end = track.End;
                }
                else
                {
                    End = track.End;
                }
            }
            if (setLength)
            {
                Length = track.Length;
            }
            if (setFlags)
            {
                if ((useInternalSetters != null) && (useInternalSetters.Contains(nameof(Flags))))
                {
                    flags.Clear();
                    flags.AddRange(track.Flags);
                }
                else
                {
                    SetFlags(track.Flags);
                }
            }
            if (setPreGap)
            {
                if ((useInternalSetters != null) && (useInternalSetters.Contains(nameof(PreGap))))
                {
                    preGap = track.PreGap;
                }
                else
                {
                    PreGap = track.PreGap;
                }
            }
            if (setPostGap)
            {
                if ((useInternalSetters != null) && (useInternalSetters.Contains(nameof(PostGap))))
                {
                    postGap = track.PostGap;
                }
                else
                {
                    PostGap = track.PostGap;
                }
            }
            if (setIsLinkedToPreviousTrack)
            {
                if ((useInternalSetters != null) && (useInternalSetters.Contains(nameof(IsLinkedToPreviousTrack))))
                {
                    isLinkedToPreviousTrack = track.IsLinkedToPreviousTrack;
                }
                else
                {
                    IsLinkedToPreviousTrack = track.IsLinkedToPreviousTrack;
                }
            }
            OnValidateablePropertyChanged();
        }

        ///<inheritdoc/>
        public void SetFlag(Flag flag, SetFlagMode flagMode)
        {
            if (flag == null)
            {
                throw new ArgumentNullException(nameof(flag));
            }
            var previousValue = flags;
            if ((flagMode == SetFlagMode.Add) && (Flags.Contains(flag) == false))
            {
                flags.Add(flag);
            }
            if ((flagMode == SetFlagMode.Remove) && (Flags.Contains(flag)))
            {
                flags.Remove(flag);
            }
            OnTraceablePropertyChanged(previousValue);
        }

        ///<inheritdoc/>
        public void SetFlags(IEnumerable<Flag> flags)
        {
            this.flags.Clear();
            this.flags.AddRange(flags);
        }


        public override string ToString()
        {
            return String.Format("({0} {1},{2} {3},{4} {5},{6} {7},{8} {9},{10} {11})", nameof(Position), Position, nameof(Artist), Artist, nameof(Title), Title, nameof(Begin), Begin, nameof(End), End, nameof(Length), Length);
        }

        protected override void Validate()
        {
            if (Position == null)
            {
                validationErrors.Add(new ValidationError(FieldReference.Create(this, nameof(Position)), ValidationErrorType.Error, "{0} has no value!", nameof(Position)));
            }
            if ((Position != null) && (Position == 0))
            {
                validationErrors.Add(new ValidationError(FieldReference.Create(this, nameof(Position)), ValidationErrorType.Error, "{0} has invalid value!", nameof(Position)));
            }
            if (String.IsNullOrEmpty(Artist) == true)
            {
                validationErrors.Add(new ValidationError(FieldReference.Create(this, nameof(Artist)), ValidationErrorType.Warning, "{0} has no value!", nameof(Artist)));
            }
            if (String.IsNullOrEmpty(Title) == true)
            {
                validationErrors.Add(new ValidationError(FieldReference.Create(this, nameof(Title)), ValidationErrorType.Warning, "{0} has no value!", nameof(Title)));
            }
            if (Begin == null)
            {
                validationErrors.Add(new ValidationError(FieldReference.Create(this, nameof(Begin)), ValidationErrorType.Error, "{0} has no value!", nameof(Begin)));
            }
            else
            {
                if (Begin < TimeSpan.Zero)
                {
                    validationErrors.Add(new ValidationError(FieldReference.Create(this, nameof(Begin)), ValidationErrorType.Error, "{0} has invalid timespan!", nameof(Begin)));
                }
            }
            if (End == null)
            {
                validationErrors.Add(new ValidationError(FieldReference.Create(this, nameof(End)), ValidationErrorType.Error, "{0} has no value!", nameof(End)));
            }
            else
            {
                if (End < TimeSpan.Zero)
                {
                    validationErrors.Add(new ValidationError(FieldReference.Create(this, nameof(End)), ValidationErrorType.Error, "{0} has invalid timespan!", nameof(End)));
                }
            }
            if (Length == null)
            {
                validationErrors.Add(new ValidationError(FieldReference.Create(this, nameof(Length)), ValidationErrorType.Error, "{0} has no value! Please check {1} and {2}.", nameof(Length), nameof(Begin), nameof(End)));
            }
            //Check track overlapping
            if (Cuesheet != null)
            {
                Cuesheet.Tracks.ToList().ForEach(x => x.RankPropertyValueChanged -= Track_RankPropertyValueChanged);
                List<Track> tracksToAttachEventHandlerTo = new();
                if (Position.HasValue)
                {
                    IEnumerable<Track> tracksWithSamePosition;
                    if (ClonedFrom != null)
                    {
                        tracksWithSamePosition = Cuesheet.Tracks.Where(x => x.Position == Position && x.Equals(this) == false && (x.Equals(ClonedFrom) == false));
                    }
                    else
                    {
                        tracksWithSamePosition = Cuesheet.Tracks.Where(x => x.Position == Position && x.Equals(this) == false);
                    }
                    if ((tracksWithSamePosition != null) && (tracksWithSamePosition.Any()))
                    {
                        tracksToAttachEventHandlerTo.AddRange(tracksWithSamePosition);
                        validationErrors.Add(new ValidationError(FieldReference.Create(this, nameof(Position)), ValidationErrorType.Error, "{0} {1} of this track is already in use by track(s) {2}!", nameof(Position), Position, String.Join(", ", tracksWithSamePosition)));
                    }
                    if (IsCloned == false)
                    {
                        Boolean addValidationError = false;
                        Track? trackAtPosition = Cuesheet.Tracks.ElementAtOrDefault((int)Position.Value - 1);
                        if (trackAtPosition != null)
                        {
                            if (trackAtPosition != this)
                            {
                                addValidationError = true;
                            }
                        }
                        else
                        {
                            // Only validate the position if the current track belongs to cuesheet since otherwise it gets validated during AddTrack
                            if ((Cuesheet.Tracks.Contains(this)) && ((Cuesheet.Tracks.ToList().IndexOf(this) + 1) != Position.Value))
                            {
                                addValidationError = true;
                            }
                        }
                        if (addValidationError)
                        {
                            validationErrors.Add(new ValidationError(FieldReference.Create(this, nameof(Position)), ValidationErrorType.Error, "{0} {1} of this track does not match track position in cuesheet. Please correct the {2} of this track to {3}!", nameof(Position), Position, nameof(Position), Cuesheet.Tracks.ToList().IndexOf(this) + 1));
                        }
                    }
                }
                if (Begin.HasValue)
                {
                    IEnumerable<Track> tracksOverlapping;
                    if (ClonedFrom != null)
                    {
                        tracksOverlapping = Cuesheet.Tracks.Where(x => Begin >= x.Begin && Begin < x.End && (x.Equals(this) == false) && (x.Equals(ClonedFrom) == false));
                    }
                    else
                    {
                        tracksOverlapping = Cuesheet.Tracks.Where(x => Begin >= x.Begin && Begin < x.End && (x.Equals(this) == false));
                    }
                    if ((tracksOverlapping != null) && tracksOverlapping.Any())
                    {
                        tracksToAttachEventHandlerTo.AddRange(tracksOverlapping);
                        validationErrors.Add(new ValidationError(FieldReference.Create(this, nameof(Begin)), ValidationErrorType.Warning, "{0} is overlapping with other track(s) ({1})!", nameof(Begin), String.Join(", ", tracksOverlapping)));
                    }
                }
                if (End.HasValue)
                {
                    IEnumerable<Track> tracksOverlapping;
                    if (ClonedFrom != null)
                    {
                        tracksOverlapping = Cuesheet.Tracks.Where(x => x.Begin < End && End <= x.End && (x.Equals(this) == false) && (x.Equals(ClonedFrom) == false));
                    }
                    else
                    {
                        tracksOverlapping = Cuesheet.Tracks.Where(x => x.Begin < End && End <= x.End && (x.Equals(this) == false));
                    }
                    if ((tracksOverlapping != null) && tracksOverlapping.Any())
                    {
                        tracksToAttachEventHandlerTo.AddRange(tracksOverlapping);
                        validationErrors.Add(new ValidationError(FieldReference.Create(this, nameof(End)), ValidationErrorType.Warning, "{0} is overlapping with other track(s) ({1})!", nameof(End), String.Join(", ", tracksOverlapping)));
                    }
                }
                tracksToAttachEventHandlerTo.ForEach(x => x.RankPropertyValueChanged += Track_RankPropertyValueChanged);
            }
        }

        protected void OnTraceablePropertyChanged(object? previousValue, [System.Runtime.CompilerServices.CallerMemberName] string propertyName = "")
        {
            TraceablePropertyChanged?.Invoke(this, new TraceablePropertiesChangedEventArgs(new TraceableChange(previousValue, propertyName)));
        }

        private void Track_RankPropertyValueChanged(object? sender, string e)
        {
            if (sender != null)
            {
                Track track = (Track)sender;
                track.RankPropertyValueChanged -= Track_RankPropertyValueChanged;
                OnValidateablePropertyChanged();
            }
            else
            {
                throw new ArgumentNullException(nameof(sender));
            }
        }

        /// <summary>
        /// Method for checking if fire of events should be done
        /// </summary>
        /// <param name="previousValue">Previous value of the property firing events</param>
        /// <param name="fireValidateablePropertyChanged">Fire OnValidateablePropertyChanged?</param>
        /// <param name="fireRankPropertyValueChanged">Fire RankPropertyValueChanged?</param>
        /// <param name="fireTraceablePropertyChanged">Fire OnTraceablePropertyChanged?</param>
        /// <param name="propertyName">Property firing the events</param>
        /// <exception cref="NullReferenceException">If propertyName can not be found, an exception is thrown.</exception>
        private void FireEvents(object? previousValue, Boolean fireValidateablePropertyChanged = true, Boolean fireRankPropertyValueChanged = true, Boolean fireTraceablePropertyChanged = true, [System.Runtime.CompilerServices.CallerMemberName] string propertyName = "")
        {
            var propertyInfo = GetType().GetProperty(propertyName);
            if (propertyInfo != null)
            {
                var propertyValue = propertyInfo.GetValue(this);
                if (Equals(propertyValue, previousValue) == false)
                {
                    if (fireValidateablePropertyChanged)
                    {
                        OnValidateablePropertyChanged();
                    }
                    if (fireRankPropertyValueChanged)
                    {
                        RankPropertyValueChanged?.Invoke(this, propertyName);
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
