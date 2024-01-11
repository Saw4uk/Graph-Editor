using System;
using System.Collections.Generic;
using UnityEngine;

namespace GraphEditor.Runtime
{
    public static class Undo
    {
        private static readonly Stack<Action> undo = new Stack<Action>();
        private static readonly Stack<Action> redo = new Stack<Action>();
    
        public static void AddActions(Action undoAction, Action redoAction)
        {
            redo.Clear();
            _AddActions(undoAction, redoAction);
        }
    
        private static void _AddActions(Action undoAction, Action redoAction)
        {
            RegisterUndoAction(() =>
            {
                undoAction.Invoke();
                RegisterRedoAction(() =>
                {
                    _AddActions(undoAction, redoAction);
                    redoAction.Invoke();
                });
            });
        }

        private static void RegisterUndoAction(Action action)
        {
            if (action == null)
                Debug.LogError("Action in null");
            undo.Push(action);
            Debug.Log($"Undo action count {undo.Count}");
        }
    
        public static void InvokeUndoAction()
        {
            if (undo.Count > 0)
            {
                undo.Pop().Invoke();
                Debug.Log($"Undo action count {undo.Count}");
            }
        }

        private static void RegisterRedoAction(Action action)
        {
            if (action == null)
                Debug.LogError("Action in null");
            redo.Push(action);
            Debug.Log($"Redo action count {redo.Count}");
        }
    
        public static void InvokeRedoAction()
        {
            if (redo.Count <= 0) return;
            redo.Pop().Invoke();
            Debug.Log($"Redo action count {redo.Count}");
        }
    }
}
