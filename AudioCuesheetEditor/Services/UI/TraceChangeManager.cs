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

namespace AudioCuesheetEditor.Services.UI;

/// <inheritdoc />
public partial class TraceChangeManager(ILogger<TraceChangeManager> logger) : ITraceChangeManager
{
    readonly ILogger<TraceChangeManager> _logger = logger;

    readonly List<TracedChanges> _undoStack = [];
    readonly List<TracedChanges> _redoStack = [];

    List<TracedChange>? _bulkEditTracedChanges;

    public event EventHandler? TracedObjectHistoryChanged;
    public event EventHandler? UndoDone;
    public event EventHandler? RedoDone;

    /// <inheritdoc />
    public bool CanUndo => _undoStack.Count > 0;

    /// <inheritdoc />
    public bool CanRedo => _redoStack.Count > 0;

    public void Reset()
    {
        _redoStack.Clear();
        _undoStack.Clear();
    }

    public void Undo()
    {
        LogUndoCalled();
        if (CanUndo)
        {
            TracedChanges? changes = null;
            while (_undoStack.Count > 0 && changes == null)
            {
                changes = _undoStack[^1];
                _ = _undoStack.Remove(changes);
                if (changes.HasTraceableObject == false)
                {
                    changes = null;
                }
            }
            if (changes != null && changes.HasTraceableObject)
            {
                var redoChanges = new List<TracedChange>();
                for (var i = changes.Changes.Count - 1; i >= 0; i--)
                {
                    var change = changes.Changes.ElementAt(i);
                    var tracedObject = change?.TraceableObject;
                    var traceAbleChange = change?.TraceableChange;
                    LogTracedObjectAndTracableChange(tracedObject, traceAbleChange);
                    if (tracedObject != null && traceAbleChange != null)
                    {
                        var propertyInfo = tracedObject.GetType().GetProperty(traceAbleChange.PropertyName);
                        var currentValue = propertyInfo!.GetValue(tracedObject);
                        redoChanges.Add(new TracedChange(tracedObject, new(currentValue, traceAbleChange.PropertyName)));
                        LogSettingProperty(traceAbleChange.PropertyName, tracedObject, currentValue, traceAbleChange.PreviousValue);
                        propertyInfo.SetValue(tracedObject, traceAbleChange.PreviousValue);
                    }
                    if (change != null)
                    {
                        _ = changes.Changes.Remove(change);
                    }
                }
                //Push the old value to redo stack
                _redoStack.Add(new TracedChanges(redoChanges));
            }
            UndoDone?.Invoke(this, EventArgs.Empty);
        }
    }

    public void Redo()
    {
        LogRedoCalled();
        if (CanRedo)
        {
            TracedChanges? changes = null;
            while (_redoStack.Count > 0 && changes == null)
            {
                changes = _redoStack[^1];
                _ = _redoStack.Remove(changes);
                if (changes.HasTraceableObject == false)
                {
                    changes = null;
                }
            }
            if (changes != null && changes.HasTraceableObject)
            {
                var undoChanges = new List<TracedChange>();
                for (var i = changes.Changes.Count - 1;i >= 0; i--) 
                {
                    var change = changes.Changes.ElementAt(i);
                    var tracedObject = change?.TraceableObject;
                    var traceAbleChange = change?.TraceableChange;
                    LogTracedObjectAndTracableChange(tracedObject, traceAbleChange);
                    if (tracedObject != null && traceAbleChange != null)
                    {
                        var propertyInfo = tracedObject.GetType().GetProperty(traceAbleChange.PropertyName);
                        var currentValue = propertyInfo!.GetValue(tracedObject);
                        undoChanges.Add(new TracedChange(tracedObject, new(currentValue, traceAbleChange.PropertyName)));
                        LogSettingProperty(traceAbleChange.PropertyName, tracedObject, currentValue, traceAbleChange.PreviousValue);
                        propertyInfo.SetValue(tracedObject, traceAbleChange.PreviousValue);
                    }
                    if (change != null)
                    {
                        _ = changes.Changes.Remove(change);
                    }
                }
                //Push the old value to redo stack
                _undoStack.Add(new TracedChanges(undoChanges));
            }
            RedoDone?.Invoke(this, EventArgs.Empty);
        }
    }

    public bool BulkEdit 
    {
        get => _bulkEditTracedChanges != null;
        set
        {
            LogSetBulkEdit(value);
            if (value)
            {
                _bulkEditTracedChanges ??= [];
            }
            else
            {
                if (_bulkEditTracedChanges?.Count > 0)
                {
                    _undoStack.Add(new TracedChanges(_bulkEditTracedChanges));
                    _redoStack.Clear();
                    TracedObjectHistoryChanged?.Invoke(this, EventArgs.Empty);
                }
                _bulkEditTracedChanges = null;
            }
        }
    }

    public void AddChange(TracedChange tracedChange)
    {
        LogAddChange(tracedChange.TraceableObject, tracedChange.TraceableChange.PropertyName, tracedChange.TraceableChange.PreviousValue);
        if (BulkEdit == false)
        {
            //Single change
            _undoStack.Add(new([tracedChange]));
            _redoStack.Clear();
            TracedObjectHistoryChanged?.Invoke(this, EventArgs.Empty);
        }
        else
        {
            //We are tracing multiple changes
            _bulkEditTracedChanges?.Add(tracedChange);
        }
    }

    public void RemoveTracedChanges(IEnumerable<object> traceables)
    {
        _ = _undoStack.RemoveAll(x => x.HasTraceableObject == false);
        _ = _undoStack.RemoveAll(x => x.Changes.Any(y => traceables.Contains(y.TraceableObject)));
        _ = _redoStack.RemoveAll(x => x.HasTraceableObject == false);
        _ = _redoStack.RemoveAll(x => x.Changes.Any(y => traceables.Contains(y.TraceableObject)));
        TracedObjectHistoryChanged?.Invoke(this, EventArgs.Empty);
    }

    [LoggerMessage(Level = LogLevel.Debug, Message = "Undo called")]
    partial void LogUndoCalled();

    [LoggerMessage(Level = LogLevel.Debug, Message = "Redo called")]
    partial void LogRedoCalled();

    [LoggerMessage(Level = LogLevel.Debug, Message = "tracedObject = {tracedObject}, traceAbleChange = {traceAbleChange}")]
    partial void LogTracedObjectAndTracableChange(object? tracedObject, object? traceAbleChange);

    [LoggerMessage(Level = LogLevel.Debug, Message = "Setting {PropertyName} on {tracedObject} from {currentValue} to {PreviousValue}")]
    partial void LogSettingProperty(string propertyName, object? tracedObject, object? currentValue, object? previousValue);

    [LoggerMessage(Level = LogLevel.Debug, Message = "Set BulkEdit called with {value}")]
    partial void LogSetBulkEdit(bool value);

    [LoggerMessage(Level = LogLevel.Debug, Message = "AddChange: {TraceableObject}, {PropertyName}, {PreviousValue}")]
    partial void LogAddChange(object? traceableObject, string? propertyName, object? previousValue);
}
