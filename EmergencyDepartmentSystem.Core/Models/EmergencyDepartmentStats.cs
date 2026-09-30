namespace EmergencyDepartmentSystem.Core.Models;

/// <summary>
/// Summary statistics of the Emergency Department at any point in time.
/// </summary>
public class EmergencyDepartmentStats
{
    public int TotalWaiting { get; set; }
    public int TotalAttended { get; set; }
    public int TotalActionsRecorded { get; set; }
    public Patient? NextPatientInQueue { get; set; }
    public PatientAction? MostRecentAction { get; set; }
    public double AverageAgeOfWaiting { get; set; }
    public TimeSpan? LongestCurrentWaitTime { get; set; }
    public DateTime GeneratedAt { get; set; } = DateTime.Now;

    public string FormattedGeneratedTime => GeneratedAt.ToString("HH:mm:ss");

    public override string ToString()
    {
        var next = NextPatientInQueue != null 
            ? $"Patient {NextPatientInQueue.PatientNumber} ({NextPatientInQueue.PatientName})" 
            : "None";
            
        var lastAction = MostRecentAction != null 
            ? $"{MostRecentAction.ActionType}: {MostRecentAction.Description}" 
            : "None";
        return $"Waiting Patients: {TotalWaiting} | Attended: {TotalAttended} | Actions Recorded: {TotalActionsRecorded} | Next: {next} | Last Action: {lastAction}";
    }
}
