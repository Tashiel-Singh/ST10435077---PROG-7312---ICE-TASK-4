using EmergencyDepartmentSystem.Core.Data;
using EmergencyDepartmentSystem.Core.DataStructures;
using EmergencyDepartmentSystem.Core.Models;

namespace EmergencyDepartmentSystem.Core.Services;

/// <summary>
/// Core implementation of the Emergency Department workflow management service.
/// </summary>
public class EmergencyDepartmentService : IEmergencyDepartmentService
{
    private readonly IPatientQueue _waitingQueue;
    private readonly IActionStack _actionHistory;
    private readonly List<Patient> _attendedPatients = new();
    private readonly Dictionary<int, Patient> _patientRegistry = new();
    private int _actionIdCounter = 0;
    private readonly object _serviceLock = new();

    public IPatientQueue WaitingQueue => _waitingQueue;
    public IActionStack ActionHistory => _actionHistory;

    public EmergencyDepartmentService()
        : this(new PatientQueue(), new ActionStack())
    {
    }

    public EmergencyDepartmentService(IPatientQueue queue, IActionStack stack)
    {
        _waitingQueue = queue ?? throw new ArgumentNullException(nameof(queue));
        _actionHistory = stack ?? throw new ArgumentNullException(nameof(stack));
    }

    public Patient RegisterPatient(int patientNumber, string patientName, int age, string reasonForVisit, DateTime? arrivalTime = null, string triageCategory = "Standard")
    {
        lock (_serviceLock)
        {
            var patient = new Patient(patientNumber, patientName, age, reasonForVisit, arrivalTime, triageCategory);
            _waitingQueue.Enqueue(patient);
            _patientRegistry[patientNumber] = patient;
            return patient;
        }
    }

    public IReadOnlyList<Patient> GetWaitingPatients()
    {
        return _waitingQueue.ToList();
    }

    public Patient CallNextPatient()
    {
        lock (_serviceLock)
        {
            var patient = _waitingQueue.Dequeue();
            patient.Status = PatientStatus.InConsultation;
            patient.AttendedTime = DateTime.Now;
            _attendedPatients.Add(patient);
            return patient;
        }
    }

    public Patient? PeekNextPatient()
    {
        return _waitingQueue.TryPeek(out var patient) ? patient : null;
    }

    public PatientAction RecordPatientAction(int patientNumber, string actionType, string description, string staffName = "Medical Staff", string? details = null)
    {
        lock (_serviceLock)
        {
            _patientRegistry.TryGetValue(patientNumber, out var patient);
            var patientName = patient?.PatientName ?? $"Patient {patientNumber}";

            _actionIdCounter++;
            var action = new PatientAction(
                actionId: _actionIdCounter,
                patientNumber: patientNumber,
                patientName: patientName,
                actionType: actionType,
                description: description,
                staffName: staffName,
                actionDetails: details,
                timestamp: DateTime.Now
            );

            _actionHistory.Push(action);
            return action;
        }
    }

    public PatientAction? ViewMostRecentAction()
    {
        return _actionHistory.TryPeek(out var action) ? action : null;
    }

    public PatientAction UndoLastAction()
    {
        return _actionHistory.Pop();
    }

    public IReadOnlyList<PatientAction> GetActionHistory()
    {
        return _actionHistory.ToList();
    }

    public EmergencyDepartmentStats GetStatistics()
    {
        lock (_serviceLock)
        {
            var waitingList = _waitingQueue.ToList();
            _waitingQueue.TryPeek(out var nextPatient);
            _actionHistory.TryPeek(out var latestAction);

            double avgAge = waitingList.Count > 0 ? waitingList.Average(p => p.Age) : 0.0;
            TimeSpan? longestWait = waitingList.Count > 0
                ? DateTime.Now - waitingList.Min(p => p.ArrivalTime)
                : null;

            return new EmergencyDepartmentStats
            {
                TotalWaiting = _waitingQueue.Count,
                TotalAttended = _attendedPatients.Count,
                TotalActionsRecorded = _actionHistory.Count,
                NextPatientInQueue = nextPatient,
                MostRecentAction = latestAction,
                AverageAgeOfWaiting = Math.Round(avgAge, 1),
                LongestCurrentWaitTime = longestWait,
                GeneratedAt = DateTime.Now
            };
        }
    }

    public IReadOnlyList<Patient> GetAttendedPatients()
    {
        lock (_serviceLock)
        {
            return _attendedPatients.ToList();
        }
    }

    public Patient? FindPatient(int patientNumber)
    {
        lock (_serviceLock)
        {
            if (_patientRegistry.TryGetValue(patientNumber, out var patient))
                return patient;

            return null;
        }
    }

    public void SeedSampleData()
    {
        lock (_serviceLock)
        {
            Reset();

            foreach (var patient in SampleEmergencyData.GetSamplePatients())
            {
                _waitingQueue.Enqueue(patient);
                _patientRegistry[patient.PatientNumber] = patient;
            }

            foreach (var action in SampleEmergencyData.GetSampleActions())
            {
                _actionHistory.Push(action);
                if (action.ActionId > _actionIdCounter)
                {
                    _actionIdCounter = action.ActionId;
                }
            }
        }
    }

    public void Reset()
    {
        lock (_serviceLock)
        {
            _waitingQueue.Clear();
            _actionHistory.Clear();
            _attendedPatients.Clear();
            _patientRegistry.Clear();
            _actionIdCounter = 0;
        }
    }
}
