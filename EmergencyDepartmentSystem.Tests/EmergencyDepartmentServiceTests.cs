using EmergencyDepartmentSystem.Core.Models;
using EmergencyDepartmentSystem.Core.Services;

namespace EmergencyDepartmentSystem.Tests;

public class EmergencyDepartmentServiceTests
{
    [Fact]
    public void RegisterPatient_EnqueuesPatientAndUpdatesRegistry()
    {
        var service = new EmergencyDepartmentService();

        var patient = service.RegisterPatient(1001, "John", 34, "Chest pain");

        Assert.NotNull(patient);
        Assert.Equal(1001, patient.PatientNumber);
        Assert.Equal("John", patient.PatientName);
        Assert.Equal(1, service.WaitingQueue.Count);

        var found = service.FindPatient(1001);
        Assert.NotNull(found);
        Assert.Equal("John", found.PatientName);
    }

    [Theory]
    [InlineData(0, "Valid", 20, "Reason")]
    [InlineData(-1, "Valid", 20, "Reason")]
    [InlineData(1001, "", 20, "Reason")]
    [InlineData(1001, "Valid", -5, "Reason")]
    [InlineData(1001, "Valid", 20, "")]
    public void RegisterPatient_ThrowsArgumentException_OnInvalidData(int id, string name, int age, string reason)
    {
        var service = new EmergencyDepartmentService();

        Assert.ThrowsAny<ArgumentException>(() => service.RegisterPatient(id, name, age, reason));
    }

    [Fact]
    public void CallNextPatient_DequeuesHeadPatient_AndMarksAttended()
    {
        var service = new EmergencyDepartmentService();
        service.RegisterPatient(1001, "John", 34, "Chest Pain");
        service.RegisterPatient(1002, "Sarah", 28, "Fracture");

        var called = service.CallNextPatient();

        Assert.Equal(1001, called.PatientNumber);
        Assert.Equal(PatientStatus.InConsultation, called.Status);
        Assert.NotNull(called.AttendedTime);
        Assert.Equal(1, service.WaitingQueue.Count);
        Assert.Single(service.GetAttendedPatients());
    }

    [Fact]
    public void CallNextPatient_ThrowsInvalidOperationException_WhenQueueEmpty()
    {
        var service = new EmergencyDepartmentService();

        Assert.Throws<InvalidOperationException>(() => service.CallNextPatient());
    }

    [Fact]
    public void PeekNextPatient_ReturnsHeadPatientWithoutRemoving()
    {
        var service = new EmergencyDepartmentService();
        service.RegisterPatient(1001, "John", 34, "Chest Pain");
        service.RegisterPatient(1002, "Sarah", 28, "Fracture");

        var next = service.PeekNextPatient();

        Assert.NotNull(next);
        Assert.Equal(1001, next.PatientNumber);
        Assert.Equal(2, service.WaitingQueue.Count);
    }

    [Fact]
    public void RecordPatientAction_PushesActionOntoStack_WithPatientNameResolved()
    {
        var service = new EmergencyDepartmentService();
        service.RegisterPatient(1001, "John", 34, "Chest Pain");

        var action = service.RecordPatientAction(1001, "Blood pressure updated", "120/80 mmHg", "Nurse Elena");

        Assert.NotNull(action);
        Assert.Equal(1, action.ActionId);
        Assert.Equal(1001, action.PatientNumber);
        Assert.Equal("John", action.PatientName);
        Assert.Equal("Blood pressure updated", action.ActionType);
        Assert.Equal(1, service.ActionHistory.Count);
    }

    [Fact]
    public void ViewMostRecentAction_ReturnsTopActionWithoutPopping()
    {
        var service = new EmergencyDepartmentService();
        service.RecordPatientAction(1001, "Blood pressure updated", "120/80");
        service.RecordPatientAction(1001, "Medication added", "Paracetamol");

        var latest = service.ViewMostRecentAction();

        Assert.NotNull(latest);
        Assert.Equal(2, latest.ActionId);
        Assert.Equal("Medication added", latest.ActionType);
        Assert.Equal(2, service.ActionHistory.Count);
    }

    [Fact]
    public void UndoLastAction_PopsTopAction_AndReturnsIt()
    {
        var service = new EmergencyDepartmentService();
        service.RecordPatientAction(1001, "Blood pressure updated", "120/80");
        service.RecordPatientAction(1001, "Medication added", "Paracetamol");

        var undone = service.UndoLastAction();

        Assert.Equal(2, undone.ActionId);
        Assert.Equal("Medication added", undone.ActionType);
        Assert.Equal(1, service.ActionHistory.Count);

        var currentTop = service.ViewMostRecentAction();
        Assert.NotNull(currentTop);
        Assert.Equal(1, currentTop.ActionId);
    }

    [Fact]
    public void UndoLastAction_ThrowsInvalidOperationException_WhenStackEmpty()
    {
        var service = new EmergencyDepartmentService();

        Assert.Throws<InvalidOperationException>(() => service.UndoLastAction());
    }

    [Fact]
    public void GetStatistics_CalculatesAccurateSystemMetrics()
    {
        var service = new EmergencyDepartmentService();
        service.RegisterPatient(1001, "John", 30, "Ache");
        service.RegisterPatient(1002, "Sarah", 40, "Pain");
        service.RecordPatientAction(1001, "Vitals", "Normal");

        var stats = service.GetStatistics();

        Assert.Equal(2, stats.TotalWaiting);
        Assert.Equal(0, stats.TotalAttended);
        Assert.Equal(1, stats.TotalActionsRecorded);
        Assert.NotNull(stats.NextPatientInQueue);
        Assert.Equal(1001, stats.NextPatientInQueue.PatientNumber);
        Assert.Equal(35.0, stats.AverageAgeOfWaiting);
    }

    [Fact]
    public void SeedSampleData_PopulatesPromptScenarioAccurately()
    {
        var service = new EmergencyDepartmentService();
        service.SeedSampleData();

        // Should have 4 patients waiting: John (1001), Sarah (1002), Michael (1003), Thand (1004)
        var waiting = service.GetWaitingPatients();
        Assert.Equal(4, waiting.Count);
        Assert.Equal("John", waiting[0].PatientName);
        Assert.Equal("Sarah", waiting[1].PatientName);
        Assert.Equal("Michael", waiting[2].PatientName);
        Assert.Equal("Thand", waiting[3].PatientName);

        // Top of action stack should be Action 4: Doctor notes added
        var latestAction = service.ViewMostRecentAction();
        Assert.NotNull(latestAction);
        Assert.Equal(4, latestAction.ActionId);
        Assert.Equal("Doctor's notes added", latestAction.ActionType);
    }

    [Fact]
    public void FullPromptScenarioWorkflow_SimulatesEntireEmergencyDay()
    {
        var service = new EmergencyDepartmentService();
        service.SeedSampleData();

        // Scenario step 1: John arrived first, so John should normally be attended to first
        var firstCalled = service.CallNextPatient();
        Assert.Equal("John", firstCalled.PatientName);
        Assert.Equal(1001, firstCalled.PatientNumber);

        // Scenario step 2: 4 actions were recorded. Nurse makes mistake and selects Undo
        // The most recent action (4: Doctor notes) must be reversed first
        var undone1 = service.UndoLastAction();
        Assert.Equal(4, undone1.ActionId);
        Assert.Equal("Doctor's notes added", undone1.ActionType);

        // Top action is now Action 3: Allergy information updated
        var topAfterUndo = service.ViewMostRecentAction();
        Assert.NotNull(topAfterUndo);
        Assert.Equal(3, topAfterUndo.ActionId);
        Assert.Equal("Allergy information updated", topAfterUndo.ActionType);

        // Next in queue to attend is Sarah (1002)
        var nextCalled = service.CallNextPatient();
        Assert.Equal("Sarah", nextCalled.PatientName);
        Assert.Equal(1002, nextCalled.PatientNumber);

        // 2 patients remain in waiting queue: Michael, then Thand
        Assert.Equal(2, service.WaitingQueue.Count);
        Assert.Equal("Michael", service.PeekNextPatient()!.PatientName);
    }
}
