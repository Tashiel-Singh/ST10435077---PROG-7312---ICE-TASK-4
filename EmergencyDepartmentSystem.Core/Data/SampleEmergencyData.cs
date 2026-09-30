using EmergencyDepartmentSystem.Core.Models;

namespace EmergencyDepartmentSystem.Core.Data;

/// <summary>
/// Pre-configured sample data directly derived from the assignment scenario specifications.
/// </summary>
public static class SampleEmergencyData
{
    public static List<Patient> GetSamplePatients()
    {
        var today = DateTime.Today;
        return new List<Patient>
        {
            new Patient(
                patientNumber: 1001,
                patientName: "John",
                age: 34,
                reasonForVisit: "Acute severe chest tightness and shortness of breath",
                arrivalTime: today.Add(new TimeSpan(8, 1, 0)),
                triageCategory: "Priority 1 - Urgent"
            ),
            new Patient(
                patientNumber: 1002,
                patientName: "Sarah",
                age: 28,
                reasonForVisit: "Fractured right wrist after sports fall",
                arrivalTime: today.Add(new TimeSpan(8, 3, 0)),
                triageCategory: "Priority 2 - Moderate"
            ),
            new Patient(
                patientNumber: 1003,
                patientName: "Michael",
                age: 52,
                reasonForVisit: "High persistent fever (39.5 C) and dehydration",
                arrivalTime: today.Add(new TimeSpan(8, 5, 0)),
                triageCategory: "Priority 2 - Moderate"
            ),
            new Patient(
                patientNumber: 1004,
                patientName: "Thand",
                age: 41,
                reasonForVisit: "Deep forearm laceration requiring sterile sutures",
                arrivalTime: today.Add(new TimeSpan(8, 7, 0)),
                triageCategory: "Priority 3 - Standard"
            )
        };
    }

    public static List<PatientAction> GetSampleActions()
    {
        var today = DateTime.Today;
        return new List<PatientAction>
        {
            new PatientAction(
                actionId: 1,
                patientNumber: 1001,
                patientName: "John",
                actionType: "Blood pressure updated",
                description: "Vitals recorded: Blood Pressure 135/88 mmHg, Pulse 82 bpm",
                staffName: "Nurse Elena",
                actionDetails: "135/88 mmHg",
                timestamp: today.Add(new TimeSpan(8, 10, 0))
            ),
            new PatientAction(
                actionId: 2,
                patientNumber: 1001,
                patientName: "John",
                actionType: "Medication added",
                description: "Administered Aspirin 300mg oral dispersible + 500ml Saline IV",
                staffName: "Dr. Gregory",
                actionDetails: "Aspirin 300mg",
                timestamp: today.Add(new TimeSpan(8, 14, 0))
            ),
            new PatientAction(
                actionId: 3,
                patientNumber: 1002,
                patientName: "Sarah",
                actionType: "Allergy information updated",
                description: "Documented severe allergy to Penicillin and latex sensitivity",
                staffName: "Nurse Elena",
                actionDetails: "Allergy: Penicillin",
                timestamp: today.Add(new TimeSpan(8, 18, 0))
            ),
            new PatientAction(
                actionId: 4,
                patientNumber: 1002,
                patientName: "Sarah",
                actionType: "Doctor's notes added",
                description: "Orthopedic consultation completed; X-Ray ordered for right distal radius",
                staffName: "Dr. Gregory",
                actionDetails: "X-Ray Right Wrist",
                timestamp: today.Add(new TimeSpan(8, 22, 0))
            )
        };
    }
}
