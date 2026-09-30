using EmergencyDepartmentSystem.Core.DataStructures;
using EmergencyDepartmentSystem.Core.Models;

namespace EmergencyDepartmentSystem.Tests;

public class PatientQueueTests
{
    [Fact]
    public void Enqueue_AddsPatientToQueue_CountIncreases()
    {
        var queue = new PatientQueue();
        var patient = new Patient(1001, "John Doe", 34, "Fever");

        queue.Enqueue(patient);

        Assert.Equal(1, queue.Count);
        Assert.False(queue.IsEmpty);
    }

    [Fact]
    public void Enqueue_ThrowsArgumentNullException_WhenPatientIsNull()
    {
        var queue = new PatientQueue();

        Assert.Throws<ArgumentNullException>(() => queue.Enqueue(null!));
    }

    [Fact]
    public void Enqueue_ThrowsInvalidOperationException_WhenPatientNumberAlreadyInQueue()
    {
        var queue = new PatientQueue();
        var p1 = new Patient(1001, "John", 30, "Headache");
        var p2 = new Patient(1001, "John Duplicate", 30, "Headache");

        queue.Enqueue(p1);

        var ex = Assert.Throws<InvalidOperationException>(() => queue.Enqueue(p2));
        Assert.Contains("already registered", ex.Message);
    }

    [Fact]
    public void Dequeue_MaintainsStrictFifoOrder()
    {
        var queue = new PatientQueue();
        var p1 = new Patient(1001, "John", 34, "Chest Pain");
        var p2 = new Patient(1002, "Sarah", 28, "Wrist Fracture");
        var p3 = new Patient(1003, "Michael", 52, "High Fever");

        queue.Enqueue(p1);
        queue.Enqueue(p2);
        queue.Enqueue(p3);

        // First in must be first out
        Assert.Same(p1, queue.Dequeue());
        Assert.Same(p2, queue.Dequeue());
        Assert.Same(p3, queue.Dequeue());
        Assert.True(queue.IsEmpty);
    }

    [Fact]
    public void Dequeue_ThrowsInvalidOperationException_WhenQueueIsEmpty()
    {
        var queue = new PatientQueue();

        var ex = Assert.Throws<InvalidOperationException>(() => queue.Dequeue());
        Assert.Contains("waiting queue is empty", ex.Message);
    }

    [Fact]
    public void TryDequeue_ReturnsFalseAndNull_WhenQueueIsEmpty()
    {
        var queue = new PatientQueue();

        bool success = queue.TryDequeue(out var patient);

        Assert.False(success);
        Assert.Null(patient);
    }

    [Fact]
    public void TryDequeue_ReturnsTrueAndPatient_WhenQueueNotEmpty()
    {
        var queue = new PatientQueue();
        var p1 = new Patient(1001, "John", 34, "Fever");
        queue.Enqueue(p1);

        bool success = queue.TryDequeue(out var patient);

        Assert.True(success);
        Assert.Same(p1, patient);
        Assert.True(queue.IsEmpty);
    }

    [Fact]
    public void Peek_ReturnsHeadPatientWithoutRemovingFromQueue()
    {
        var queue = new PatientQueue();
        var p1 = new Patient(1001, "John", 34, "Fever");
        var p2 = new Patient(1002, "Sarah", 28, "Pain");
        queue.Enqueue(p1);
        queue.Enqueue(p2);

        var peeked = queue.Peek();

        Assert.Same(p1, peeked);
        Assert.Equal(2, queue.Count); // Count must remain unchanged
    }

    [Fact]
    public void Peek_ThrowsInvalidOperationException_WhenQueueIsEmpty()
    {
        var queue = new PatientQueue();

        Assert.Throws<InvalidOperationException>(() => queue.Peek());
    }

    [Fact]
    public void TryPeek_ReturnsFalseAndNull_WhenQueueIsEmpty()
    {
        var queue = new PatientQueue();

        bool success = queue.TryPeek(out var patient);

        Assert.False(success);
        Assert.Null(patient);
    }

    [Fact]
    public void TryPeek_ReturnsTrueAndHeadPatient_WhenQueueHasItems()
    {
        var queue = new PatientQueue();
        var p1 = new Patient(1001, "John", 34, "Fever");
        queue.Enqueue(p1);

        bool success = queue.TryPeek(out var patient);

        Assert.True(success);
        Assert.Same(p1, patient);
        Assert.Equal(1, queue.Count);
    }

    [Fact]
    public void ContainsPatient_ReturnsCorrectResult()
    {
        var queue = new PatientQueue();
        queue.Enqueue(new Patient(1001, "John", 34, "Fever"));

        Assert.True(queue.ContainsPatient(1001));
        Assert.False(queue.ContainsPatient(9999));
    }

    [Fact]
    public void ToList_ReturnsPatientsInArrivalOrder()
    {
        var queue = new PatientQueue();
        var p1 = new Patient(1001, "John", 34, "Fever");
        var p2 = new Patient(1002, "Sarah", 28, "Fracture");
        queue.Enqueue(p1);
        queue.Enqueue(p2);

        var list = queue.ToList();

        Assert.Equal(2, list.Count);
        Assert.Equal(1001, list[0].PatientNumber);
        Assert.Equal(1002, list[1].PatientNumber);
    }

    [Fact]
    public void Clear_EmptiesAllPatientsFromQueue()
    {
        var queue = new PatientQueue();
        queue.Enqueue(new Patient(1001, "John", 34, "Fever"));
        queue.Enqueue(new Patient(1002, "Sarah", 28, "Fracture"));

        queue.Clear();

        Assert.Equal(0, queue.Count);
        Assert.True(queue.IsEmpty);
    }
}