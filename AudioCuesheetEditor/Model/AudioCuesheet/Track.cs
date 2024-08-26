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
using AudioCuesheetEditor.Model.UI;
using System.Text.Json.Serialization;

namespace AudioCuesheetEditor.Model.AudioCuesheet
{
    public enum SetFlagMode
    {
        Add,
        Remove
    }

    public class Track : Validateable<Track>, ICuesheetEntity, ITraceable, ITrack
    {
        public static readonly List<String> AllPropertyNames = [nameof(IsLinkedToPreviousTrack), nameof(Position), nameof(Artist), nameof(Title), nameof(Begin), nameof(End), nameof(Flags), nameof(PreGap), nameof(PostGap), nameof(Length)];

        private uint? position;
        private String? artist;
        private String? title;
        private TimeSpan? begin;
        private TimeSpan? end;
        private TimeSpan? _length;
        private List<Flag> flags = [];
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

        public Track() { }

        /// <summary>
        /// Create object with copied values from input
        /// </summary>
        /// <param name="track">Object to copy values from</param>
        /// /// <param name="copyCuesheetReference">Copy cuesheet reference from track also?</param>
        public Track(ITrack track, Boolean copyCuesheetReference = true) : this()
        {
            CopyValues(track, copyCuesheetReference);
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
            set { var previousValue = end; end = value; FireEvents(previousValue,  propertyName: nameof(End)); }
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
                var previousValue = Length;
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
                FireEvents(previousValue, fireRankPropertyValueChanged: false, fireTraceablePropertyChanged: false);
            }
        }
        [JsonInclude]
        public IReadOnlyCollection<Flag> Flags
        {
            get { return flags.AsReadOnly(); }
            private set
            {
                flags = [.. value];
            }
        }
        [JsonIgnore]
        public Cuesheet? Cuesheet 
        {
            get => cuesheet;
            set 
            {
                var previousValue = cuesheet;
                Boolean setValue = true;
                if (value != null)
                {
                    setValue = value.Tracks.Contains(this);
                }
                if (setValue)
                {
                    cuesheet = value;
                    FireEvents(previousValue, fireValidateablePropertyChanged: false, fireRankPropertyValueChanged: false, propertyName: nameof(Cuesheet));
                }
            }
        }

        /// <summary>
        /// Indicates that this track has been cloned from another track and is a transparent proxy
        /// </summary>
        [JsonIgnore]
        public Boolean IsCloned { get { return ClonedFrom != null; } }
        /// <summary>
        /// Get the original track that this track has been cloned from. Can be null on original objects
        /// </summary>
        public Track? ClonedFrom { get; set; }
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
        public void CopyValues(ITrack track, Boolean setCuesheet = true, Boolean setIsLinkedToPreviousTrack = true, Boolean setPosition = true, Boolean setArtist = true, Boolean setTitle = true, Boolean setBegin = true, Boolean setEnd = true, Boolean setLength = false, Boolean setFlags = true, Boolean setPreGap = true, Boolean setPostGap = true, IEnumerable<String>? useInternalSetters = null)
        {
            if (setCuesheet && (track is Track cuesheetTrack))
            {
                if ((useInternalSetters != null) && (useInternalSetters.Contains(nameof(Cuesheet))))
                {
                    cuesheet = cuesheetTrack.Cuesheet;
                }
                else
                {
                    Cuesheet = cuesheetTrack.Cuesheet;
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
                //TODO
                //if ((useInternalSetters != null) && (useInternalSetters.Contains(nameof(Flags))))
                //{
                //    flags.Clear();
                //    flags.AddRange(track.Flags);
                //}
                //else
                //{
                //    SetFlags(track.Flags);
                //}
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
            if (setIsLinkedToPreviousTrack && (track is Track cuesheetTrack2))
            {
                if ((useInternalSetters != null) && (useInternalSetters.Contains(nameof(IsLinkedToPreviousTrack))))
                {
                    isLinkedToPreviousTrack = cuesheetTrack2.IsLinkedToPreviousTrack;
                }
                else
                {
                    IsLinkedToPreviousTrack = cuesheetTrack2.IsLinkedToPreviousTrack;
                }
            }
        }

        ///<inheritdoc/>
        public void SetFlag(Flag flag, SetFlagMode flagMode)
        {
            var previousValue = flags;
            if ((flagMode == SetFlagMode.Add) && (Flags.Contains(flag) == false))
            {
                flags.Add(flag);
            }
            if ((flagMode == SetFlagMode.Remove) && Flags.Contains(flag))
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

        protected override ValidationResult Validate(string property)
        {
            ValidationStatus validationStatus = ValidationStatus.NoValidation;
            List<ValidationMessage>? validationMessages = null;
            switch (property)
            {
                case nameof(Position):
                    validationStatus = ValidationStatus.Success;
                    if (Position == null)
                    {
                        validationMessages ??= [];
                        validationMessages.Add(new ValidationMessage("{0} has no value!", nameof(Position)));
                    }
                    else
                    {
                        if (Position == 0)
                        {
                            validationMessages ??= [];
                            validationMessages.Add(new ValidationMessage("{0} may not be 0!", nameof(Position)));
                        }
                        else
                        {
                            // Check correct track position
                            if ((IsCloned == false) && (Cuesheet != null))
                            {
                                var positionTrackShouldHave = Cuesheet.Tracks.OrderBy(x => x.Begin ?? TimeSpan.MaxValue).ThenBy(x => x.Position).ToList().IndexOf(this) + 1;
                                if (positionTrackShouldHave != Position)
                                {
                                    validationMessages ??= [];
                                    validationMessages.Add(new ValidationMessage("Track({0},{1},{2},{3},{4}) does not have the correct position '{5}'!", Position, Artist ?? String.Empty, Title ?? String.Empty, Begin != null ? Begin : String.Empty, End != null ? End : String.Empty, positionTrackShouldHave));
                                }
                            }
                        }
                    }
                    break;
                case nameof(Begin):
                    validationStatus = ValidationStatus.Success;
                    if (Begin == null)
                    {
                        validationMessages ??= [];
                        validationMessages.Add(new ValidationMessage("{0} has no value!", nameof(Begin)));
                    }
                    else
                    {
                        if (Begin < TimeSpan.Zero)
                        {
                            validationMessages ??= [];
                            validationMessages.Add(new ValidationMessage("{0} must be equal or greater zero!", nameof(Begin)));
                        }
                        if (Begin > End)
                        {
                            validationMessages ??= [];
                            validationMessages.Add(new ValidationMessage("{0} should be less than or equal '{1}'!", nameof(Begin), nameof(End)));
                        }
                    }
                    break;
                case nameof(End):
                    validationStatus = ValidationStatus.Success;
                    if (End == null)
                    {
                        validationMessages ??= [];
                        validationMessages.Add(new ValidationMessage("{0} has no value!", nameof(End)));
                    }
                    else
                    {
                        if (End < TimeSpan.Zero)
                        {
                            validationMessages ??= [];
                            validationMessages.Add(new ValidationMessage("{0} must be equal or greater zero!", nameof(End)));
                        }
                        if (End < Begin)
                        {
                            validationMessages ??= [];
                            validationMessages.Add(new ValidationMessage("{0} should be greater than or equal '{1}'!", nameof(End), nameof(Begin)));
                        }
                    }
                    break;
                case nameof(Length):
                    validationStatus = ValidationStatus.Success;
                    if (Length == null)
                    {
                        validationMessages ??= [];
                        validationMessages.Add(new ValidationMessage("{0} has no value!", nameof(Length)));
                    }
                    break;
            }
            return ValidationResult.Create(validationStatus, validationMessages);
        }

        protected void OnTraceablePropertyChanged(object? previousValue, [System.Runtime.CompilerServices.CallerMemberName] string propertyName = "")
        {
            TraceablePropertyChanged?.Invoke(this, new TraceablePropertiesChangedEventArgs(new TraceableChange(previousValue, propertyName)));
        }

        /// <summary>
        /// Method for checking if fire of events should be done
        /// </summary>
        /// <param name="previousValue">Previous value of the property firing events</param>
        /// <param name="fireValidateablePropertyChanged">Fire OnValidateablePropertyChanged?</param>
        /// <param name="fireRankPropertyValueChanged">Fire RankPropertyValueChanged?</param>
        /// <param name="fireTraceablePropertyChanged">Fire OnTraceablePropertyChanged?</param>
        /// <exception cref="NullReferenceException">If propertyName can not be found, an exception is thrown.</exception>
        /// <param name="propertyName">Property firing the events</param>
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
                        OnValidateablePropertyChanged(propertyName);
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
