namespace EmergencyDepartmentSystem.Core.Models;

public enum PatientStatus
{
    Waiting,
    InConsultation,
    Discharged,
    Cancelled
}

/// <summary>
/// Represents an emergency department patient in the waiting queue.
/// </summary>
public class Patient
{
    public int PatientNumber { get; set; }
    public string PatientName { get; set; } = string.Empty;
    public int Age { get; set; }
    public string ReasonForVisit { get; set; } = string.Empty;
    public DateTime ArrivalTime { get; set; }
    public PatientStatus Status { get; set; } = PatientStatus.Waiting;
    public string TriageCategory { get; set; } = "Standard";
    public DateTime? AttendedTime { get; set; }

    public Patient()
    {
        ArrivalTime = DateTime.Now;
    }

    public Patient(int patientNumber, string patientName, int age, string reasonForVisit, DateTime? arrivalTime = null, string triageCategory = "Standard")
    {
        if (patientNumber <= 0)
            throw new ArgumentOutOfRangeException(nameof(patientNumber), "Patient number must be a positive integer.");
        
        if (string.IsNullOrWhiteSpace(patientName))
            throw new ArgumentException("Patient name cannot be null or empty.", nameof(patientName));

        if (age < 0 || age > 130)
            throw new ArgumentOutOfRangeException(nameof(age), "Age must be between 0 and 130.");

        if (string.IsNullOrWhiteSpace(reasonForVisit))
            throw new ArgumentException("Reason for visit cannot be null or empty.", nameof(reasonForVisit));

        PatientNumber = patientNumber;
        PatientName = patientName.Trim();
        Age = age;
        ReasonForVisit = reasonForVisit.Trim();
        ArrivalTime = arrivalTime ?? DateTime.Now;
        TriageCategory = string.IsNullOrWhiteSpace(triageCategory) ? "Standard" : triageCategory.Trim();
        Status = PatientStatus.Waiting;
    }

    public string FormattedArrivalTime => ArrivalTime.ToString("HH:mm");

    public override string ToString()
    {
        return $"{FormattedArrivalTime} - Patient {PatientNumber} - {PatientName} (Age: {Age}) | Reason: {ReasonForVisit}";
    }
}
