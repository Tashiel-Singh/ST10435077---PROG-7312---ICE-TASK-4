using EmergencyDepartmentSystem.Core.Models;

namespace EmergencyDepartmentSystem.Tests;

public class DomainModelTests
{
    [Fact]
    public void Patient_DefaultConstructor_SetsDefaultValues()
    {
        var p = new Patient();
        Assert.Equal(PatientStatus.Waiting, p.Status);
        Assert.Equal("Standard", p.TriageCategory);
        Assert.True(p.ArrivalTime <= DateTime.Now);
    }

    [Fact]
    public void Patient_ParameterizedConstructor_SetsPropertiesProperly()
    {
        var time = new DateTime(2026, 9, 30, 8, 1, 0);
        var p = new Patient(1001, "John Doe", 34, "Chest Pain", time, "Urgent");

        Assert.Equal(1001, p.PatientNumber);
        Assert.Equal("John Doe", p.PatientName);
        Assert.Equal(34, p.Age);
        Assert.Equal("Chest Pain", p.ReasonForVisit);
        Assert.Equal("08:01", p.FormattedArrivalTime);
        Assert.Equal("Urgent", p.TriageCategory);
    }

    [Fact]
    public void Patient_ToString_ContainsEssentialInformation()
    {
        var p = new Patient(1001, "John", 34, "Chest Pain");
        var text = p.ToString();

        Assert.Contains("1001", text);
        Assert.Contains("John", text);
        Assert.Contains("Chest Pain", text);
    }

    [Fact]
    public void PatientAction_Constructor_SetsPropertiesProperly()
    {
        var action = new PatientAction(1, 1001, "John", "Blood pressure updated", "120/80 mmHg", "Nurse Elena");

        Assert.Equal(1, action.ActionId);
        Assert.Equal(1001, action.PatientNumber);
        Assert.Equal("John", action.PatientName);
        Assert.Equal("Blood pressure updated", action.ActionType);
        Assert.Equal("120/80 mmHg", action.Description);
        Assert.Equal("Nurse Elena", action.StaffName);
    }

    [Fact]
    public void PatientAction_ToString_ContainsActionDetails()
    {
        var action = new PatientAction(1, 1001, "John", "Medication added", "Paracetamol 500mg");
        var text = action.ToString();

        Assert.Contains("Action #1", text);
        Assert.Contains("1001", text);
        Assert.Contains("Medication added", text);
    }

    [Fact]
    public void EmergencyStats_ToString_ContainsSummaryMetrics()
    {
        var stats = new EmergencyDepartmentStats
        {
            TotalWaiting = 3,
            TotalAttended = 1,
            TotalActionsRecorded = 4,
            NextPatientInQueue = new Patient(1001, "John", 34, "Pain"),
            MostRecentAction = new PatientAction(4, 1002, "Sarah", "Doctor's notes added", "X-Ray") 
        };

        var text = stats.ToString();
        Assert.Contains("Waiting Patients: 3", text);
        Assert.Contains("Attended: 1", text);
        Assert.Contains("Actions Recorded: 4", text);
        Assert.Contains("John", text);
        Assert.Contains("Doctor's notes added", text);
    }
}