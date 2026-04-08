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
    public class CuesheetManager(ITraceChangeManager traceChangeManager) : ICuesheetManager
    {
        private readonly ITraceChangeManager _traceChangeManager = traceChangeManager;

        public event EventHandler<Cuesheet>? IsRecordingChanged;

        //TODO: Tests

        /// <inheritdoc/>
        public void SetProperty<TProperty>(Cuesheet cuesheet, Expression<Func<Cuesheet, TProperty>> propertyExpression, TProperty value)
        {
            SetValue(cuesheet, propertyExpression, value);
        }

        /// <inheritdoc/>
        public Result IsRecordingPossible(Cuesheet cuesheet)
        {
            if (cuesheet.Tracks.Count != 0)
            {
                return Result.Failure(new Error(ErrorType.NotPossible, "Cuesheet already contains tracks!"));
            }
            return Result.Success();
        }

        /// <inheritdoc/>
        public Result StartRecording(Cuesheet cuesheet)
        {
            if (cuesheet.IsRecording == true)
            {
                return Result.Failure(new Error(ErrorType.NotPossible, "Record is already running!"));
            }
            cuesheet.RecordingStart = DateTime.UtcNow;
            IsRecordingChanged?.Invoke(this, cuesheet);
            return Result.Success();
        }

        /// <inheritdoc/>
        public void StopRecording(Cuesheet cuesheet)
        {
            var lastTrack = cuesheet.Tracks.LastOrDefault();
            if ((lastTrack != null) && cuesheet.RecordingStart.HasValue)
            {
                lastTrack.End = DateTime.UtcNow - cuesheet.RecordingStart.Value;
            }
            cuesheet.RecordingStart = null;
            IsRecordingChanged?.Invoke(this, cuesheet);
        }

        void SetValue<TProperty>(Cuesheet cuesheet, Expression<Func<Cuesheet, TProperty>> propertyExpression, TProperty value)
        {
            if (propertyExpression.Body is not MemberExpression memberExpression)
            {
                throw new ArgumentException("Expression must be a property");
            }

            if (memberExpression.Member is not PropertyInfo propertyInfo)
            {
                throw new ArgumentException("Member is not a property");
            }

            var previousValue = (TProperty?)propertyInfo.GetValue(cuesheet);
            if (Equals(previousValue, value))
            {
                return;
            }

            propertyInfo.SetValue(cuesheet, value);

            _traceChangeManager.AddChange(new(cuesheet, new(previousValue, propertyInfo.Name)));
        }
    }
}
