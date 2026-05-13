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
using AudioCuesheetEditor.Services.UI;
using System.Linq.Expressions;
using System.Reflection;

namespace AudioCuesheetEditor.Services.AudioCuesheet
{
    /// <inheritdoc/>
    public class TrackManager(ITraceChangeManager traceChangeManager) : ITrackManager
    {
        private readonly ITraceChangeManager _traceChangeManager = traceChangeManager;

        /// <inheritdoc/>
        public void SetProperty<TProperty>(Track track, Expression<Func<Track, TProperty>> propertyExpression, TProperty value)
        {
            SetValue(track, propertyExpression, value);
        }

        /// <inheritdoc/>
        public Track Clone(ITrack track)
        {
            Boolean setLength = true;
            if (track.Begin.HasValue && track.End.HasValue)
            {
                setLength = false;
            }
            var clone = new Track()
            {
                IsLinkedToPreviousTrack = track.IsLinkedToPreviousTrack,
                Position = track.Position,
                Artist = track.Artist,
                Title = track.Title,
                Begin = track.Begin,
                End = track.End,
                Flags = track.Flags,
                PreGap = track.PreGap,
                PostGap = track.PostGap
            };
            if (setLength)
            {
                clone.Length = track.Length;
            }
            return clone;
        }

        /// <inheritdoc/>
        public void CopyValues(ITrack source, Track target, bool setIsLinkedToPreviousTrack = true, bool setPosition = true, bool setArtist = true, bool setTitle = true, bool setBegin = true, bool setEnd = true, bool setLength = true, bool setFlags = true, bool setPreGap = true, bool setPostGap = true)
        {
            if (setIsLinkedToPreviousTrack)
            {
                SetValue(target, x => x.IsLinkedToPreviousTrack, source.IsLinkedToPreviousTrack);
            }
            if (setPosition)
            {
                SetValue(target, x => x.Position, source.Position);
            }
            if (setArtist)
            {
                SetValue(target, x => x.Artist, source.Artist);
            }
            if (setTitle)
            {
                SetValue(target, x => x.Title, source.Title);
            }
            if (setBegin)
            {
                SetValue(target, x => x.Begin, source.Begin );
            }
            if (setEnd)
            {
                SetValue(target, x => x.End, source.End);
            }
            if (setLength)
            {
                SetValue(target, x => x.Length, source.Length);
            }
            if (setFlags)
            {
                SetValue(target, x => x.Flags, source.Flags);
            }
            if (setPreGap)
            {
                SetValue(target, x => x.PreGap, source.PreGap);
            }
            if (setPostGap)
            {
                SetValue(target, x=> x.PostGap, source.PostGap);
            }
        }

        /// <inheritdoc/>
        public Track? GetPreviousLinkedTrack(Track track)
        {
            if (track.IsLinkedToPreviousTrack == false)
            {
                return null;
            }
            if (track.Position.HasValue && (track.Cuesheet?.Tracks.All(x => x.Position.HasValue) == true))
            {
                return track.Cuesheet?.Tracks.LastOrDefault(x => x.Position == track.Position - 1 && Equals(x, track) == false);
            }
            if (track.Begin.HasValue)
            {
                return track.Cuesheet?.Tracks.OrderBy(x => x.End).LastOrDefault(x => x.End <= track.Begin && Equals(x, track) == false);
            }
            return null;
        }

        /// <inheritdoc/>
        public Track? GetNextLinkedTrack(Track track)
        {
            if (track.Position.HasValue && (track.Cuesheet?.Tracks.All(x => x.Position.HasValue) == true))
            {
                return track.Cuesheet?.Tracks.OrderBy(x => x.Begin).FirstOrDefault(x => x.Position >= track.Position.Value + 1 && x.IsLinkedToPreviousTrack == true && Equals(x, track) == false);
            }
            if (track.End.HasValue)
            {
                return track.Cuesheet?.Tracks.OrderBy(x => x.Begin).LastOrDefault(x => x.Begin <= track.End && x.IsLinkedToPreviousTrack == true && Equals(x, track) == false);
            }
            return null;
        }

        /// <inheritdoc/>
        public void RecalculateLinkedTracksProperties(Track track)
        {
            if (track.Cuesheet != null)
            {
                var previousTrack = GetPreviousLinkedTrack(track);
                if (previousTrack != null)
                {
                    if (track.Position.HasValue == false && previousTrack.Position.HasValue && (track.Position != previousTrack.Position.Value + 1))
                    {
                        SetValue(track, x => x.Position, (ushort?)(previousTrack.Position + 1));
                    }
                    if (previousTrack.End.HasValue && (track.Begin != previousTrack.End))
                    {
                        SetValue(track, x => x.Begin, previousTrack.End);
                    }
                    if ((previousTrack.End.HasValue == false) && track.Begin.HasValue)
                    {
                        SetValue(previousTrack, x => x.End, track.Begin);
                    }
                }
                var nextTrack = GetNextLinkedTrack(track);
                if (nextTrack != null)
                {
                    if (track.Position.HasValue)
                    {
                        SetValue(nextTrack, x => x.Position, (ushort?)(track.Position + 1));
                    }
                    SetValue(nextTrack, x => x.Begin, track.End);
                }
            }
        }

        void SetValue<TProperty>(Track track, Expression<Func<Track, TProperty>> propertyExpression, TProperty value)
        {
            if (propertyExpression.Body is not MemberExpression memberExpression)
            {
                throw new ArgumentException("Expression must be a property");
            }

            if (memberExpression.Member is not PropertyInfo propertyInfo)
            {
                throw new ArgumentException("Member is not a property");
            }

            var previousValue = (TProperty?)propertyInfo.GetValue(track);
            if (Equals(previousValue, value))
            {
                return;
            }

            propertyInfo.SetValue(track, value);
            _traceChangeManager.AddChange(new(track, new(previousValue, propertyInfo.Name)));
        }
    }
}