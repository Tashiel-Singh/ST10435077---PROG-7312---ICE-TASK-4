using EmergencyDepartmentSystem.Core.Models;

namespace EmergencyDepartmentSystem.Core.DataStructures;

/// <summary>
/// Interface defining Last-In, First-Out (LIFO) stack operations for patient record action history and reversal (Undo).
/// </summary>
public interface IActionStack : IEnumerable<PatientAction>
{
    /// <summary>
    /// Pushes an action onto the top of the stack (LIFO Push).
    /// </summary>
    void Push(PatientAction action);

    /// <summary>
    /// Pops and returns the most recent action from the top of the stack (LIFO Pop / Undo).
    /// </summary>
    PatientAction Pop();

    /// <summary>
    /// Safely attempts to pop the most recent action without throwing if the stack is empty.
    /// </summary>
    bool TryPop(out PatientAction? action);

    /// <summary>
    /// Inspects the topmost action on the stack without removing it (LIFO Peek).
    /// </summary>
    PatientAction Peek();

    /// <summary>
    /// Safely attempts to inspect the most recent action without throwing if the stack is empty.
    /// </summary>
    bool TryPeek(out PatientAction? action);

    /// <summary>
    /// Total count of actions currently stored in the history stack.
    /// </summary>
    int Count { get; }

    /// <summary>
    /// Indicates whether the action history stack contains zero actions.
    /// </summary>
    bool IsEmpty { get; }

    /// <summary>
    /// Returns an ordered list of actions from top of stack (most recent) down to bottom (oldest).
    /// </summary>
    IReadOnlyList<PatientAction> ToList();

    /// <summary>
    /// Clears all actions from the stack.
    /// </summary>
    void Clear();
}
