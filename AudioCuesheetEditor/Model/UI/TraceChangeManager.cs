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
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace AudioCuesheetEditor.Model.UI
{
    /// <summary>
    /// Class for tracing changes on an object
    /// </summary>
    public class TracedChange
    {
        private readonly WeakReference<ITraceable> _tracedObject;
        public TracedChange(ITraceable traceableObject, TraceableChange traceableChange)
        {
            _tracedObject = new WeakReference<ITraceable>(traceableObject, false);
            TraceableChange = traceableChange;
        }
        public ITraceable? TraceableObject
        {
            get
            {
                if (_tracedObject.TryGetTarget(out var traceable))
                {
                    return traceable;
                }
                return null;
            }
        }

        public TraceableChange TraceableChange { get; }
    }

    public class TracedChanges
    {
        private readonly Stack<TracedChange> _tracedChanges;

        public TracedChanges(IEnumerable<TracedChange> changes)
        {
            _tracedChanges = new(changes);
        }

        public Stack<TracedChange> Changes => _tracedChanges;
        public Boolean HasTraceableObject { get { return Changes.Any(x => x.TraceableObject != null); } }
    }

    /// <summary>
    /// Manager for Undo and Redo operations on objects.
    /// </summary>
    public class TraceChangeManager
    {
        private readonly ILogger<TraceChangeManager> _logger;

        private readonly Stack<TracedChanges> undoStack = new();
        private readonly Stack<TracedChanges> redoStack = new();

        private List<TracedChange>? bulkEditTracedChanges;

        public event EventHandler? TracedObjectHistoryChanged;
        public event EventHandler? UndoDone;
        public event EventHandler? RedoDone;

        public Boolean CurrentlyHandlingRedoOrUndoChanges { get; private set; }
        /// <summary>
        /// Is Undo() currently possible (are there any changes)?
        /// </summary>
        public Boolean CanUndo
        {
            get
            {
                return undoStack.Count > 0;
            }
        }

        /// <summary>
        /// Is Redo() currently possible (are there any changes)?
        /// </summary>
        public Boolean CanRedo
        {
            get
            {
                return redoStack.Count > 0;
            }
        }

        public TraceChangeManager(ILogger<TraceChangeManager> logger)
        {
            CurrentlyHandlingRedoOrUndoChanges = false;
            _logger = logger;
        }

        public void TraceChanges(ITraceable traceable)
        {
            traceable.TraceablePropertyChanged += Traceable_TraceablePropertyChanged;
        }

        public void Reset()
        {
            ResetStack(redoStack);
            ResetStack(undoStack);
        }

        public void Undo()
        {
            _logger.LogDebug("Undo called");
            if (CanUndo)
            {
                CurrentlyHandlingRedoOrUndoChanges = true; 
                TracedChanges? changes = null;
                while ((undoStack.Count > 0) && (changes == null))
                {
                    changes = undoStack.Pop();
                    if (changes.HasTraceableObject == false)
                    {
                        changes = null;
                    }
                }
                if ((changes != null) && changes.HasTraceableObject)
                {
                    var redoChanges = new List<TracedChange>();
                    do
                    {
                        var change = changes.Changes.Pop();
                        var tracedObject = change?.TraceableObject;
                        var traceAbleChange = change?.TraceableChange;
                        _logger.LogDebug("tracedObject = {tracedObject}, traceAbleChange = {traceAbleChange}", tracedObject, traceAbleChange);
                        if ((tracedObject != null) && (traceAbleChange != null))
                        {
                            var propertyInfo = tracedObject.GetType().GetProperty(traceAbleChange.PropertyName);
                            if (propertyInfo != null)
                            {
                                var currentValue = propertyInfo.GetValue(tracedObject);
                                redoChanges.Add(new TracedChange(tracedObject, new TraceableChange(currentValue, traceAbleChange.PropertyName)));
                                _logger.LogDebug("Setting {PropertyName} on {tracedObject} from {currentValue} to {PreviousValue}", traceAbleChange.PropertyName, tracedObject, currentValue, traceAbleChange.PreviousValue);
                                propertyInfo.SetValue(tracedObject, traceAbleChange.PreviousValue);
                            }
                            else
                            {
                                throw new NullReferenceException(String.Format("Property {0} could not be found!", traceAbleChange.PropertyName));
                            }
                        }
                    }
                    while (changes.Changes.Count > 0);
                    //Push the old value to redo stack
                    redoStack.Push(new TracedChanges(redoChanges));
                }
                CurrentlyHandlingRedoOrUndoChanges = false;
                UndoDone?.Invoke(this, EventArgs.Empty);
            }
        }

        public void Redo()
        {
            _logger.LogDebug("Redo called");
            if (CanRedo)
            {
                CurrentlyHandlingRedoOrUndoChanges = true;
                TracedChanges? changes = null;
                while ((redoStack.Count > 0) && (changes == null))
                {
                    changes = redoStack.Pop();
                    if (changes.HasTraceableObject == false)
                    {
                        changes = null;
                    }
                }
                if ((changes != null) && changes.HasTraceableObject)
                {
                    var undoChanges = new List<TracedChange>();
                    do
                    {
                        var change = changes.Changes.Pop();
                        var tracedObject = change?.TraceableObject;
                        var traceAbleChange = change?.TraceableChange;
                        _logger.LogDebug("tracedObject = {tracedObject}, traceAbleChange = {traceAbleChange}", tracedObject, traceAbleChange);
                        if ((tracedObject != null) && (traceAbleChange != null))
                        {
                            var propertyInfo = tracedObject.GetType().GetProperty(traceAbleChange.PropertyName);
                            if (propertyInfo != null)
                            {
                                var currentValue = propertyInfo.GetValue(tracedObject);
                                undoChanges.Add(new TracedChange(tracedObject, new TraceableChange(currentValue, traceAbleChange.PropertyName)));
                                _logger.LogDebug("Setting {PropertyName} on {tracedObject} from {currentValue} to {PreviousValue}", traceAbleChange.PropertyName, tracedObject, currentValue, traceAbleChange.PreviousValue);
                                propertyInfo.SetValue(tracedObject, traceAbleChange.PreviousValue);
                            }
                            else
                            {
                                throw new NullReferenceException(String.Format("Property {0} could not be found!", traceAbleChange.PropertyName));
                            }
                        }
                    }
                    while (changes.Changes.Count > 0);
                    //Push the old value to redo stack
                    undoStack.Push(new TracedChanges(undoChanges));
                }
                CurrentlyHandlingRedoOrUndoChanges = false;
                RedoDone?.Invoke(this, EventArgs.Empty);
            }
        }

        public Boolean BulkEdit 
        {
            get => bulkEditTracedChanges != null;
            set
            {
                _logger.LogDebug("Set BulkEdit called with {value}", value);
                if (value)
                {
                    bulkEditTracedChanges = new();
                }
                else
                {
                    if (bulkEditTracedChanges != null)
                    {
                        undoStack.Push(new TracedChanges(bulkEditTracedChanges));
                        TracedObjectHistoryChanged?.Invoke(this, EventArgs.Empty);
                        bulkEditTracedChanges = null;
                    }
                }
            }
        }

        private void ResetStack(Stack<TracedChanges> stack)
        {
            while (stack.Count > 0)
            {
                var tracedChange = stack.Pop();
                while (tracedChange.Changes.Count > 0)
                {
                    var change = tracedChange.Changes.Pop();
                    if (change.TraceableObject != null)
                    {
                        change.TraceableObject.TraceablePropertyChanged -= Traceable_TraceablePropertyChanged;
                    }
                }
            }
            stack.Clear();
        }

        private void Traceable_TraceablePropertyChanged(object? sender, TraceablePropertiesChangedEventArgs e)
        {
            _logger.LogDebug("Traceable_TraceablePropertyChanged called with {sender}, {PropertyName}, {PreviousValue}", sender, e.TraceableChange.PropertyName, e.TraceableChange.PreviousValue);
            _logger.LogDebug("CurrentlyHandlingRedoOrUndoChanges = {CurrentlyHandlingRedoOrUndoChanges}", CurrentlyHandlingRedoOrUndoChanges);
            if (CurrentlyHandlingRedoOrUndoChanges == false)
            {
                if (sender != null)
                {
                    _logger.LogDebug("BulkEdit = {BulkEdit}", BulkEdit);
                    if (BulkEdit == false)
                    {
                        //Single change
                        var changes = new TracedChanges(new List<TracedChange>() { new TracedChange((ITraceable)sender, e.TraceableChange) });
                        undoStack.Push(changes);
                        redoStack.Clear();
                        TracedObjectHistoryChanged?.Invoke(this, EventArgs.Empty);
                    }
                    else
                    {
                        //We are tracing multiple changes
                        bulkEditTracedChanges?.Add(new TracedChange((ITraceable)sender, e.TraceableChange));
                    }
                }
                else
                {
                    throw new ArgumentNullException(nameof(sender));
                }
            }
        }
    }
}
