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

        //TODO: Tests

        /// <inheritdoc/>
        public void SetProperty<TProperty>(Track track, Expression<Func<Track, TProperty>> propertyExpression, TProperty value)
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
            //TODO: If IsLinkedToPreviousTrack is true set also position, begin and end

            _traceChangeManager.AddChange(new(track, new(previousValue, propertyInfo.Name)));
        }

        /// <inheritdoc/>
        public Track Clone(ITrack track)
        {
            var clone = new Track();
            CopyValues(track, clone);
            return clone;
        }

        /// <inheritdoc/>
        public void CopyValues(ITrack source, Track target, bool setIsLinkedToPreviousTrack = true, bool setPosition = true, bool setArtist = true, bool setTitle = true, bool setBegin = true, bool setEnd = true, bool setLength = false, bool setFlags = true, bool setPreGap = true, bool setPostGap = true)
        {
            //TODO: Create change for ITraceChangeManager
            if (setIsLinkedToPreviousTrack)
            {
                target.IsLinkedToPreviousTrack = source.IsLinkedToPreviousTrack;
            }
            if (setPosition)
            {
                target.Position = source.Position;
            }
            if (setArtist)
            {
                target.Artist = source.Artist;
            }
            if (setTitle)
            {
                target.Title = source.Title;
            }
            if (setBegin)
            {
                target.Begin = source.Begin;
            }
            if (setEnd)
            {
                target.End = source.End;
            }
            if (setLength)
            {
                target.Length = source.Length;
            }
            if (setFlags)
            {
                target.Flags = source.Flags;
            }
            if (setPreGap)
            {
                target.PreGap = source.PreGap;
            }
            if (setPostGap)
            {
                target.PostGap = source.PostGap;
            }
        }
    }
}