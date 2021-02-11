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
using AudioCuesheetEditor.Model.Reflection;
using AudioCuesheetEditor.Shared.ResourceFiles;
using Microsoft.Extensions.Localization;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace AudioCuesheetEditor.Model.AudioCuesheet
{
    public class Track : Validateable, ITrack<Cuesheet>
    {
        private uint? position;
        private String artist;
        private String title;
        private TimeSpan? begin;
        private TimeSpan? end;
        private readonly List<Flag> flags = new List<Flag>();
        private Track clonedFrom = null;

        /// <summary>
        /// A property with influence to position of this track in cuesheet has been changed. Name of the property changed is provided in event arguments.
        /// </summary>
        public event EventHandler<String> RankPropertyValueChanged;
        
        /// <summary>
        /// Create object with copied values from input
        /// </summary>
        /// <param name="track">Object to copy values from</param>
        public Track(ITrack<Cuesheet> track)
        {
            CopyValues(track);
        }

        public Track()
        {
            Validate();
        }

        public uint? Position 
        {
            get { return position; }
            set { position = value; OnValidateablePropertyChanged(); RankPropertyValueChanged?.Invoke(this, nameof(Position)); }
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
        public TimeSpan? Begin 
        {
            get { return begin; }
            set { begin = value; OnValidateablePropertyChanged(); RankPropertyValueChanged?.Invoke(this, nameof(Begin)); }
        }
        public TimeSpan? End 
        {
            get { return end; }
            set { end = value; OnValidateablePropertyChanged(); RankPropertyValueChanged?.Invoke(this, nameof(End)); }
        }
        public TimeSpan? Length 
        {
            get
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
            set
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
                        if (Begin.Value > End.Value)
                        {
                            End = Begin.Value + value;
                        }
                    }
                    else
                    {
                        if (End.HasValue == false)
                        {
                            End = Begin.Value + value;
                        }
                        if (Begin.HasValue == false)
                        {
                            Begin = End.Value - value;
                        }
                    }
                }
                OnValidateablePropertyChanged();
            }
        }
        public IReadOnlyCollection<Flag> Flags => flags.AsReadOnly();

        public Cuesheet Cuesheet { get; set; }

        /// <summary>
        /// Indicates that this track has been cloned from another track and is a transparent proxy
        /// </summary>
        public Boolean IsCloned { get { return clonedFrom != null; } }
        /// <summary>
        /// Get the original track that this track has been cloned from. Can be null on original objects
        /// </summary>
        public Track ClonedFrom
        {
            get { return clonedFrom; }
            private set { clonedFrom = value; OnValidateablePropertyChanged(); }
        }

        public String GetDisplayNameLocalized(IStringLocalizer<Localization> localizer)
        {
            String identifierString = null;
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
        /// Copies values from input object to this object
        /// </summary>
        /// <param name="track">Object to copy values from</param>
        public void CopyValues(ITrack<Cuesheet> track)
        {
            if (track == null)
            {
                throw new ArgumentNullException(nameof(track));
            }
            Cuesheet = track.Cuesheet;
            //We use the internal properties because we only want to set the values, everything around like validation or automatic calculation doesn't need to be fired
            position = track.Position;
            artist = track.Artist;
            title = track.Title;
            begin = track.Begin;
            end = track.End;
            flags.Clear();
            flags.AddRange(track.Flags);
            OnValidateablePropertyChanged();
        }

        protected override void Validate()
        {
            if (Position == null)
            {
                validationErrors.Add(new ValidationError(FieldReference.Create(this, nameof(Position)), ValidationErrorType.Error, "HasNoValue", nameof(Position)));
            }
            if ((Position != null) && (Position == 0))
            {
                validationErrors.Add(new ValidationError(FieldReference.Create(this, nameof(Position)), ValidationErrorType.Error, "HasInvalidValue", nameof(Position)));
            }
            if (String.IsNullOrEmpty(Artist) == true)
            {
                validationErrors.Add(new ValidationError(FieldReference.Create(this, nameof(Artist)), ValidationErrorType.Warning, "HasNoValue", nameof(Artist)));
            }
            if (String.IsNullOrEmpty(Title) == true)
            {
                validationErrors.Add(new ValidationError(FieldReference.Create(this, nameof(Title)), ValidationErrorType.Warning, "HasNoValue", nameof(Title)));
            }
            if (Begin == null)
            {
                validationErrors.Add(new ValidationError(FieldReference.Create(this, nameof(Begin)), ValidationErrorType.Error, "HasNoValue", nameof(Begin)));
            }
            else
            {
                if (Begin < TimeSpan.Zero)
                {
                    validationErrors.Add(new ValidationError(FieldReference.Create(this, nameof(Begin)), ValidationErrorType.Error, "HasInvalidBegin"));
                }
            }
            if (End == null)
            {
                validationErrors.Add(new ValidationError(FieldReference.Create(this, nameof(End)), ValidationErrorType.Error, "HasNoValue", nameof(End)));
            }
            else
            {
                if (End < TimeSpan.Zero)
                {
                    validationErrors.Add(new ValidationError(FieldReference.Create(this, nameof(End)), ValidationErrorType.Error, "HasInvalidEnd"));
                }
            }
            if (Length == null)
            {
                validationErrors.Add(new ValidationError(FieldReference.Create(this, nameof(Length)), ValidationErrorType.Error, "LengthHasNoValue"));
            }
            //Check track overlapping
            if (Cuesheet != null)
            {
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
                        foreach(var track in tracksWithSamePosition)
                        {
                            track.RankPropertyValueChanged += Track_RankPropertyValueChanged;
                        }
                        validationErrors.Add(new ValidationError(FieldReference.Create(this, nameof(Position)), ValidationErrorType.Error, "PositionAlreadyInUse", Position, String.Join(", ", tracksWithSamePosition)));
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
                        foreach (var track in tracksOverlapping)
                        {
                            track.RankPropertyValueChanged += Track_RankPropertyValueChanged;
                        }
                        validationErrors.Add(new ValidationError(FieldReference.Create(this, nameof(Begin)), ValidationErrorType.Warning, "TimeIsOverlapping", nameof(Begin), String.Join(", ", tracksOverlapping)));
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
                        foreach (var track in tracksOverlapping)
                        {
                            track.RankPropertyValueChanged += Track_RankPropertyValueChanged;
                        }
                        validationErrors.Add(new ValidationError(FieldReference.Create(this, nameof(End)), ValidationErrorType.Warning, "TimeIsOverlapping", nameof(End), String.Join(", ", tracksOverlapping)));
                    }
                }
            }
        }

        private void Track_RankPropertyValueChanged(object sender, string e)
        {
            Track track = (Track)sender;
            track.RankPropertyValueChanged -= Track_RankPropertyValueChanged;
            OnValidateablePropertyChanged();
        }

        ///<inheritdoc/>
        public void SetFlag(Flag flag, SetFlagMode flagMode)
        {
            if (flag == null)
            {
                throw new ArgumentNullException(nameof(flag));
            }
            if ((flagMode == SetFlagMode.Add) && (Flags.Contains(flag) == false))
            {
                flags.Add(flag);
            }
            if ((flagMode == SetFlagMode.Remove) && (Flags.Contains(flag)))
            {
                flags.Remove(flag);
            }
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
    }
}
