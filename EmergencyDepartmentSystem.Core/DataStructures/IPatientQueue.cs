using EmergencyDepartmentSystem.Core.Models;

namespace EmergencyDepartmentSystem.Core.DataStructures;

/// <summary>
/// Interface defining First-In, First-Out (FIFO) queue operations for emergency patients.
/// </summary>
public interface IPatientQueue : IEnumerable<Patient>
{
    /// <summary>
    /// Adds a patient to the back of the queue (FIFO Enqueue).
    /// </summary>
    void Enqueue(Patient patient);

    /// <summary>
    /// Removes and returns the patient at the front of the queue (FIFO Dequeue).
    /// </summary>
    Patient Dequeue();

    /// <summary>
    /// Attempts to remove and return the patient at the front of the queue.
    /// </summary>
    bool TryDequeue(out Patient? patient);

    /// <summary>
    /// Inspects the patient at the front without removing them (FIFO Peek).
    /// </summary>
    Patient Peek();

    /// <summary>
    /// Attempts to inspect the front patient without throwing an exception.
    /// </summary>
    bool TryPeek(out Patient? patient);

    /// <summary>
    /// Total number of patients currently waiting in the queue.
    /// </summary>
    int Count { get; }

    /// <summary>
    /// Indicates whether the queue is currently empty.
    /// </summary>
    bool IsEmpty { get; }

    /// <summary>
    /// Checks if a patient with the specified ID number is in the waiting queue.
    /// </summary>
    bool ContainsPatient(int patientNumber);

    /// <summary>
    /// Returns an ordered list of patients from head (first in line) to tail.
    /// </summary>
    IReadOnlyList<Patient> ToList();

    /// <summary>
    /// Clears all waiting patients from the queue.
    /// </summary>
    void Clear();
}
