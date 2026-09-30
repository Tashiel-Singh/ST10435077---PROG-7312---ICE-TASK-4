using EmergencyDepartmentSystem.Core.DataStructures;
using EmergencyDepartmentSystem.Core.Models;

namespace EmergencyDepartmentSystem.Tests;

public class ActionStackTests
{
    [Fact]
    public void Push_AddsActionToTopOfStack_CountIncreases()
    {
        var stack = new ActionStack();
        var action = new PatientAction(1, 1001, "John", "Blood pressure updated", "120/80 mmHg");

        stack.Push(action);

        Assert.Equal(1, stack.Count);
        Assert.False(stack.IsEmpty);
    }

    [Fact]
    public void Push_ThrowsArgumentNullException_WhenActionIsNull()
    {
        var stack = new ActionStack();

        Assert.Throws<ArgumentNullException>(() => stack.Push(null!));
    }

    [Fact]
    public void Pop_MaintainsStrictLifoOrder_ReversesMostRecentFirst()
    {
        var stack = new ActionStack();
        var a1 = new PatientAction(1, 1001, "John", "Blood pressure updated", "120/80");
        var a2 = new PatientAction(2, 1001, "John", "Medication added", "Paracetamol 500mg");
        var a3 = new PatientAction(3, 1002, "Sarah", "Allergy information updated", "Penicillin allergy");
        var a4 = new PatientAction(4, 1002, "Sarah", "Doctor notes added", "X-Ray ordered");

        stack.Push(a1);
        stack.Push(a2);
        stack.Push(a3);
        stack.Push(a4);

        // Last pushed (Action 4) must be popped first
        Assert.Same(a4, stack.Pop());
        Assert.Same(a3, stack.Pop());
        Assert.Same(a2, stack.Pop());
        Assert.Same(a1, stack.Pop());
        Assert.True(stack.IsEmpty);
    }

    [Fact]
    public void Pop_ThrowsInvalidOperationException_WhenStackIsEmpty()
    {
        var stack = new ActionStack();

        var ex = Assert.Throws<InvalidOperationException>(() => stack.Pop());
        Assert.Contains("action history stack is empty", ex.Message);
    }

    [Fact]
    public void TryPop_ReturnsFalseAndNull_WhenStackIsEmpty()
    {
        var stack = new ActionStack();

        bool success = stack.TryPop(out var action);

        Assert.False(success);
        Assert.Null(action);
    }

    [Fact]
    public void TryPop_ReturnsTrueAndPopsTopAction_WhenStackNotEmpty()
    {
        var stack = new ActionStack();
        var a1 = new PatientAction(1, 1001, "John", "Blood pressure updated", "120/80");
        stack.Push(a1);

        bool success = stack.TryPop(out var action);

        Assert.True(success);
        Assert.Same(a1, action);
        Assert.True(stack.IsEmpty);
    }

    [Fact]
    public void Peek_ReturnsTopActionWithoutRemovingFromStack()
    {
        var stack = new ActionStack();
        var a1 = new PatientAction(1, 1001, "John", "Blood pressure updated", "120/80");
        var a2 = new PatientAction(2, 1001, "John", "Medication added", "Paracetamol");
        stack.Push(a1);
        stack.Push(a2);

        var peeked = stack.Peek();

        Assert.Same(a2, peeked);
        Assert.Equal(2, stack.Count); // Count must remain intact
    }

    [Fact]
    public void Peek_ThrowsInvalidOperationException_WhenStackIsEmpty()
    {
        var stack = new ActionStack();

        Assert.Throws<InvalidOperationException>(() => stack.Peek());
    }

    [Fact]
    public void TryPeek_ReturnsFalseAndNull_WhenStackIsEmpty()
    {
        var stack = new ActionStack();

        bool success = stack.TryPeek(out var action);

        Assert.False(success);
        Assert.Null(action);
    }

    [Fact]
    public void TryPeek_ReturnsTrueAndTopAction_WhenStackHasItems()
    {
        var stack = new ActionStack();
        var a1 = new PatientAction(1, 1001, "John", "Blood pressure updated", "120/80");
        stack.Push(a1);

        bool success = stack.TryPeek(out var action);

        Assert.True(success);
        Assert.Same(a1, action);
        Assert.Equal(1, stack.Count);
    }

    [Fact]
    public void ToList_ReturnsActionsInLifoOrder_NewestToOldest()
    {
        var stack = new ActionStack();
        var a1 = new PatientAction(1, 1001, "John", "Blood pressure updated", "120/80");
        var a2 = new PatientAction(2, 1001, "John", "Medication added", "Paracetamol");
        stack.Push(a1);
        stack.Push(a2);

        var list = stack.ToList();

        Assert.Equal(2, list.Count);
        Assert.Equal(2, list[0].ActionId); // Newest is at index 0
        Assert.Equal(1, list[1].ActionId);
    }

    [Fact]
    public void Clear_EmptiesAllActionsFromStack()
    {
        var stack = new ActionStack();
        stack.Push(new PatientAction(1, 1001, "John", "BP", "120/80"));
        stack.Push(new PatientAction(2, 1001, "John", "Meds", "Paracetamol"));

        stack.Clear();

        Assert.Equal(0, stack.Count);
        Assert.True(stack.IsEmpty);
    }
}