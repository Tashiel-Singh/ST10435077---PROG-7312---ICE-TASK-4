using System.Collections;
using EmergencyDepartmentSystem.Core.Models;

namespace EmergencyDepartmentSystem.Core.DataStructures;

/// <summary>
/// Thread-safe implementation of a First-In, First-Out (FIFO) queue for emergency patients.
/// Time Complexity: Enqueue O(1), Dequeue O(1), Peek O(1).
/// </summary>
public class PatientQueue : IPatientQueue
{
    private readonly Queue<Patient> _queue;
    private readonly object _syncRoot = new();

    public PatientQueue()
    {
        _queue = new Queue<Patient>();
    }

    public PatientQueue(IEnumerable<Patient> initialPatients)
    {
        _queue = new Queue<Patient>(initialPatients ?? throw new ArgumentNullException(nameof(initialPatients)));
    }

    public int Count
    {
        get
        {
            lock (_syncRoot)
            {
                return _queue.Count;
            }
        }
    }

    public bool IsEmpty => Count == 0;

    public void Enqueue(Patient patient)
    {
        ArgumentNullException.ThrowIfNull(patient);

        lock (_syncRoot)
        {
            if (_queue.Any(p => p.PatientNumber == patient.PatientNumber))
            {
                throw new InvalidOperationException($"Patient with number {patient.PatientNumber} is already registered in the waiting queue.");
            }

            _queue.Enqueue(patient);
        }
    }

    public Patient Dequeue()
    {
        lock (_syncRoot)
        {
            if (_queue.Count == 0)
            {
                throw new InvalidOperationException("Cannot call next patient: The waiting queue is empty.");
            }

            return _queue.Dequeue();
        }
    }

    public bool TryDequeue(out Patient? patient)
    {
        lock (_syncRoot)
        {
            if (_queue.Count > 0)
            {
                patient = _queue.Dequeue();
                return true;
            }

            patient = null;
            return false;
        }
    }

    public Patient Peek()
    {
        lock (_syncRoot)
        {
            if (_queue.Count == 0)
            {
                throw new InvalidOperationException("Cannot peek patient: The waiting queue is empty.");
            }

            return _queue.Peek();
        }
    }

    public bool TryPeek(out Patient? patient)
    {
        lock (_syncRoot)
        {
            if (_queue.Count > 0)
            {
                patient = _queue.Peek();
                return true;
            }

            patient = null;
            return false;
        }
    }

    public bool ContainsPatient(int patientNumber)
    {
        lock (_syncRoot)
        {
            return _queue.Any(p => p.PatientNumber == patientNumber);
        }
    }

    public IReadOnlyList<Patient> ToList()
    {
        lock (_syncRoot)
        {
            return _queue.ToList();
        }
    }

    public void Clear()
    {
        lock (_syncRoot)
        {
            _queue.Clear();
        }
    }

    public IEnumerator<Patient> GetEnumerator()
    {
        List<Patient> snapshot;
        lock (_syncRoot)
        {
            snapshot = _queue.ToList();
        }
        return snapshot.GetEnumerator();
    }

    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
}
