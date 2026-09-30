using System.Collections;
using EmergencyDepartmentSystem.Core.Models;

namespace EmergencyDepartmentSystem.Core.DataStructures;

/// <summary>
/// Thread-safe implementation of a Last-In, First-Out (LIFO) stack for patient record action history.
/// Time Complexity: Push O(1), Pop O(1), Peek O(1).
/// </summary>
public class ActionStack : IActionStack
{
    private readonly Stack<PatientAction> _stack;
    private readonly object _syncRoot = new();

    public ActionStack()
    {
        _stack = new Stack<PatientAction>();
    }

    public ActionStack(IEnumerable<PatientAction> initialActions)
    {
        _stack = new Stack<PatientAction>();
        if (initialActions != null)
        {
            foreach (var action in initialActions)
            {
                _stack.Push(action);
            }
        }
    }

    public int Count
    {
        get
        {
            lock (_syncRoot)
            {
                return _stack.Count;
            }
        }
    }

    public bool IsEmpty => Count == 0;

    public void Push(PatientAction action)
    {
        ArgumentNullException.ThrowIfNull(action);

        lock (_syncRoot)
        {
            _stack.Push(action);
        }
    }

    public PatientAction Pop()
    {
        lock (_syncRoot)
        {
            if (_stack.Count == 0)
            {
                throw new InvalidOperationException("Cannot undo action: The action history stack is empty.");
            }

            return _stack.Pop();
        }
    }

    public bool TryPop(out PatientAction? action)
    {
        lock (_syncRoot)
        {
            if (_stack.Count > 0)
            {
                action = _stack.Pop();
                return true;
            }

            action = null;
            return false;
        }
    }

    public PatientAction Peek()
    {
        lock (_syncRoot)
        {
            if (_stack.Count == 0)
            {
                throw new InvalidOperationException("Cannot view most recent action: No actions have been recorded yet.");
            }

            return _stack.Peek();
        }
    }

    public bool TryPeek(out PatientAction? action)
    {
        lock (_syncRoot)
        {
            if (_stack.Count > 0)
            {
                action = _stack.Peek();
                return true;
            }

            action = null;
            return false;
        }
    }

    public IReadOnlyList<PatientAction> ToList()
    {
        lock (_syncRoot)
        {
            // In .NET Stack<T>.ToList() yields items from Top to Bottom (LIFO order)
            return _stack.ToList();
        }
    }

    public void Clear()
    {
        lock (_syncRoot)
        {
            _stack.Clear();
        }
    }

    public IEnumerator<PatientAction> GetEnumerator()
    {
        List<PatientAction> snapshot;
        lock (_syncRoot)
        {
            snapshot = _stack.ToList();
        }
        return snapshot.GetEnumerator();
    }

    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
}
