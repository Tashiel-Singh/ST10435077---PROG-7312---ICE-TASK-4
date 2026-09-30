namespace EmergencyDepartmentSystem.Core.Models;

/// <summary>
/// Represents a clinical action recorded on a patient record, stored in a LIFO history stack.
/// </summary>
public class PatientAction
{
    public int ActionId { get; set; }
    public int PatientNumber { get; set; }
    public string PatientName { get; set; } = string.Empty;
    public string ActionType { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public DateTime Timestamp { get; set; }
    public string StaffName { get; set; } = "Medical Staff";
    public string? ActionDetails { get; set; }

    public PatientAction()
    {
        Timestamp = DateTime.Now;
    }

    public PatientAction(
        int actionId,
        int patientNumber,
        string patientName,
        string actionType,
        string description,
        string staffName = "Medical Staff",
        string? actionDetails = null,
        DateTime? timestamp = null)
    {
        if (actionId <= 0)
            throw new ArgumentOutOfRangeException(nameof(actionId), "Action ID must be positive.");

        if (patientNumber <= 0)
            throw new ArgumentOutOfRangeException(nameof(patientNumber), "Patient number must be positive.");

        if (string.IsNullOrWhiteSpace(actionType))
            throw new ArgumentException("Action type cannot be empty.", nameof(actionType));

        if (string.IsNullOrWhiteSpace(description))
            throw new ArgumentException("Description cannot be empty.", nameof(description));

        ActionId = actionId;
        PatientNumber = patientNumber;
        PatientName = string.IsNullOrWhiteSpace(patientName) ? $"Patient {patientNumber}" : patientName.Trim();
        ActionType = actionType.Trim();
        Description = description.Trim();
        StaffName = string.IsNullOrWhiteSpace(staffName) ? "Medical Staff" : staffName.Trim();
        ActionDetails = actionDetails?.Trim();
        Timestamp = timestamp ?? DateTime.Now;
    }

    public string FormattedTimestamp => Timestamp.ToString("HH:mm:ss");

    public override string ToString()
    {
        return $"[{FormattedTimestamp}] Action #{ActionId} | Patient {PatientNumber} ({PatientName}) | {ActionType}: {Description} (by {StaffName})";
    }
}
