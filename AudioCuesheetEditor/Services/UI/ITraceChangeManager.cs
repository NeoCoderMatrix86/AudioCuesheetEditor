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
using AudioCuesheetEditor.Model.UI;

namespace AudioCuesheetEditor.Services.UI
{
    /// <summary>
    /// Manager for Undo and Redo operations on objects.
    /// </summary>
    public interface ITraceChangeManager
    {
        public event EventHandler? TracedObjectHistoryChanged;
        public event EventHandler? UndoDone;
        public event EventHandler? RedoDone;
        /// <summary>
        /// Is Undo() currently possible (are there any changes)?
        /// </summary>
        public bool CanUndo { get; }
        /// <summary>
        /// Is Redo() currently possible (are there any changes)?
        /// </summary>
        public bool CanRedo { get; }
        public bool BulkEdit { get; set; }
        public void TraceChanges(ITraceable traceable);
        public void Reset();
        public void Undo();
        public void Redo();
        public void MergeLastEditWithEdit(Func<TracedChanges, bool> targetEdit);
        public void RemoveTracedChanges(IEnumerable<ITraceable> traceables);
    }
}
