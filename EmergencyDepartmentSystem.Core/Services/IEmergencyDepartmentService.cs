using EmergencyDepartmentSystem.Core.DataStructures;
using EmergencyDepartmentSystem.Core.Models;

namespace EmergencyDepartmentSystem.Core.Services;

/// <summary>
/// Service interface managing the Emergency Department workflow, coordinating
/// FIFO patient waiting queue and LIFO clinical action history stack.
/// </summary>
public interface IEmergencyDepartmentService
{
    IPatientQueue WaitingQueue { get; }
    IActionStack ActionHistory { get; }

    Patient RegisterPatient(int patientNumber, string patientName, int age, string reasonForVisit, DateTime? arrivalTime = null, string triageCategory = "Standard");
    IReadOnlyList<Patient> GetWaitingPatients();
    Patient CallNextPatient();
    Patient? PeekNextPatient();
    PatientAction RecordPatientAction(int patientNumber, string actionType, string description, string staffName = "Medical Staff", string? details = null);
    PatientAction? ViewMostRecentAction();
    PatientAction UndoLastAction();
    IReadOnlyList<PatientAction> GetActionHistory();
    EmergencyDepartmentStats GetStatistics();
    IReadOnlyList<Patient> GetAttendedPatients();
    Patient? FindPatient(int patientNumber);
    void SeedSampleData();
    void Reset();
}
