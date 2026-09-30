using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Threading;
using EmergencyDepartmentSystem.Core.Models;
using EmergencyDepartmentSystem.Core.Services;

namespace EmergencyDepartmentSystem.App;

public partial class MainWindow : Window
{
    private readonly IEmergencyDepartmentService _service;
    private readonly DispatcherTimer _clockTimer;

    public MainWindow()
    {
        InitializeComponent();
        _service = new EmergencyDepartmentService();
        _service.SeedSampleData();

        _clockTimer = new DispatcherTimer
        {
            Interval = TimeSpan.FromSeconds(1)
        };
        _clockTimer.Tick += (s, e) => ClockTextBlock.Text = DateTime.Now.ToString("HH:mm:ss");
        _clockTimer.Start();

        RefreshDashboard();
    }

    private void RefreshDashboard()
    {
        var stats = _service.GetStatistics();
        var waiting = _service.GetWaitingPatients();
        var actions = _service.GetActionHistory();

        // Update KPIs
        WaitingCountText.Text = stats.TotalWaiting.ToString();
        AttendedCountText.Text = stats.TotalAttended.ToString();
        ActionsCountText.Text = stats.TotalActionsRecorded.ToString();

        if (stats.NextPatientInQueue != null)
        {
            NextPatientText.Text = $"#{stats.NextPatientInQueue.PatientNumber} {stats.NextPatientInQueue.PatientName}";
            NextPatientDetailText.Text = $"{stats.NextPatientInQueue.ReasonForVisit} ({stats.NextPatientInQueue.FormattedArrivalTime})";
        }
        else
        {
            NextPatientText.Text = "None";
            NextPatientDetailText.Text = "Queue is empty";
        }

        if (stats.MostRecentAction != null)
        {
            LatestActionTypeText.Text = stats.MostRecentAction.ActionType;
            LatestActionPatientText.Text = $"Patient #{stats.MostRecentAction.PatientNumber} ({stats.MostRecentAction.PatientName})";
        }
        else
        {
            LatestActionTypeText.Text = "None";
            LatestActionPatientText.Text = "Stack is empty";
        }

        // Build view models for Waiting DataGrid with Position Label
        var waitingViewModels = waiting.Select((p, index) => new WaitingPatientViewModel
        {
            PositionLabel = index == 0 ? "1 [NEXT]" : (index + 1).ToString(),
            FormattedArrivalTime = p.FormattedArrivalTime,
            PatientNumber = p.PatientNumber,
            PatientName = p.PatientName,
            Age = p.Age,
            TriageCategory = p.TriageCategory,
            ReasonForVisit = p.ReasonForVisit
        }).ToList();
        WaitingPatientsDataGrid.ItemsSource = waitingViewModels;

        // Build view models for Actions DataGrid with Stack Order Label
        var actionViewModels = actions.Select((a, index) => new ActionHistoryViewModel
        {
            StackLabel = index == 0 ? "TOP [LIFO]" : $"#{index + 1}",
            FormattedTimestamp = a.FormattedTimestamp,
            ActionId = a.ActionId,
            PatientDisplay = $"#{a.PatientNumber} {a.PatientName}",
            ActionType = a.ActionType,
            Description = a.Description,
            StaffName = a.StaffName
        }).ToList();
        ActionHistoryDataGrid.ItemsSource = actionViewModels;

        StatusBarText.Text = $"System Updated: {DateTime.Now:HH:mm:ss} | {stats.TotalWaiting} waiting in FIFO queue | {stats.TotalActionsRecorded} actions in LIFO stack";
    }

    private void OnCallNextPatientClicked(object sender, RoutedEventArgs e)
    {
        try
        {
            var called = _service.CallNextPatient();
            ShowBanner($"[CALLING PATIENT] #{called.PatientNumber} {called.PatientName} (Age: {called.Age}) -> Consultation Room 1! Arrived at {called.FormattedArrivalTime}.", isSuccess: true);
            RefreshDashboard();
        }
        catch (Exception ex)
        {
            ShowBanner($"Cannot call patient: {ex.Message}", isSuccess: false);
        }
    }

    private void OnRegisterSubmitClicked(object sender, RoutedEventArgs e)
    {
        if (!int.TryParse(RegPatientNumberBox.Text.Trim(), out int patNum) || patNum <= 0)
        {
            MessageBox.Show("Please enter a valid positive patient number.", "Validation Error", MessageBoxButton.OK, MessageBoxImage.Warning);
            return;
        }

        string name = RegPatientNameBox.Text.Trim();
        if (string.IsNullOrWhiteSpace(name))
        {
            MessageBox.Show("Please enter the patient's name.", "Validation Error", MessageBoxButton.OK, MessageBoxImage.Warning);
            return;
        }

        if (!int.TryParse(RegAgeBox.Text.Trim(), out int age) || age < 0 || age > 130)
        {
            MessageBox.Show("Please enter a valid age between 0 and 130.", "Validation Error", MessageBoxButton.OK, MessageBoxImage.Warning);
            return;
        }

        string reason = RegReasonBox.Text.Trim();
        if (string.IsNullOrWhiteSpace(reason))
        {
            MessageBox.Show("Please enter a reason for the visit.", "Validation Error", MessageBoxButton.OK, MessageBoxImage.Warning);
            return;
        }

        string triage = (RegTriageCombo.SelectedItem as ComboBoxItem)?.Content?.ToString() ?? "Priority 3 - Standard";

        try
        {
            var registered = _service.RegisterPatient(patNum, name, age, reason, DateTime.Now, triage);
            ShowBanner($"[REGISTERED] Patient #{registered.PatientNumber} ({registered.PatientName}) added to waiting queue (FIFO position #{_service.WaitingQueue.Count}).", isSuccess: true);
            RegPatientNumberBox.Text = (patNum + 1).ToString();
            RegPatientNameBox.Text = string.Empty;
            RegReasonBox.Text = string.Empty;
            RegisterExpander.IsExpanded = false;
            RefreshDashboard();
        }
        catch (Exception ex)
        {
            MessageBox.Show(ex.Message, "Registration Error", MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }

    private void OnUndoLastActionClicked(object sender, RoutedEventArgs e)
    {
        try
        {
            var undone = _service.UndoLastAction();
            ShowBanner($"[ACTION REVERSED / UNDONE] Action #{undone.ActionId} ({undone.ActionType}: '{undone.Description}' for Patient #{undone.PatientNumber}) has been popped from the stack.", isSuccess: true);
            RefreshDashboard();
        }
        catch (Exception ex)
        {
            ShowBanner($"Cannot undo action: {ex.Message}", isSuccess: false);
        }
    }

    private void OnPeekActionClicked(object sender, RoutedEventArgs e)
    {
        var top = _service.ViewMostRecentAction();
        if (top != null)
        {
            MessageBox.Show($"TOP OF ACTION STACK (LIFO):\n\n" +
                            $"Action ID: #{top.ActionId}\n" +
                            $"Timestamp: {top.FormattedTimestamp}\n" +
                            $"Patient: #{top.PatientNumber} ({top.PatientName})\n" +
                            $"Type: {top.ActionType}\n" +
                            $"Description: {top.Description}\n" +
                            $"Staff: {top.StaffName}\n\n" +
                            $"Note: Pressing 'Undo Last Action' will reverse this exact action.",
                            "Inspect Top of Stack", MessageBoxButton.OK, MessageBoxImage.Information);
        }
        else
        {
            MessageBox.Show("The action history stack is currently empty. No actions to peek.", "Stack Empty", MessageBoxButton.OK, MessageBoxImage.Information);
        }
    }

    private void OnRecordActionSubmitClicked(object sender, RoutedEventArgs e)
    {
        if (!int.TryParse(ActPatientNumberBox.Text.Trim(), out int patNum) || patNum <= 0)
        {
            MessageBox.Show("Please enter a valid patient number.", "Validation Error", MessageBoxButton.OK, MessageBoxImage.Warning);
            return;
        }

        string category = (ActCategoryCombo.SelectedItem as ComboBoxItem)?.Content?.ToString() ?? "Clinical Action";
        string description = ActDescriptionBox.Text.Trim();
        if (string.IsNullOrWhiteSpace(description))
        {
            MessageBox.Show("Please enter a description of the action.", "Validation Error", MessageBoxButton.OK, MessageBoxImage.Warning);
            return;
        }

        string staff = ActStaffBox.Text.Trim();
        if (string.IsNullOrWhiteSpace(staff))
            staff = "Medical Staff";

        try
        {
            var action = _service.RecordPatientAction(patNum, category, description, staff);
            ShowBanner($"[ACTION RECORDED] Action #{action.ActionId} pushed onto history stack for Patient #{action.PatientNumber} ({action.PatientName}).", isSuccess: true);
            ActDescriptionBox.Text = string.Empty;
            RecordExpander.IsExpanded = false;
            RefreshDashboard();
        }
        catch (Exception ex)
        {
            MessageBox.Show(ex.Message, "Action Error", MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }

    private void OnSeedDataClicked(object sender, RoutedEventArgs e)
    {
        _service.SeedSampleData();
        ShowBanner("Assignment scenario seed data reloaded (John, Sarah, Michael, Thand + 4 actions).", isSuccess: true);
        RefreshDashboard();
    }

    private void OnResetAllClicked(object sender, RoutedEventArgs e)
    {
        var result = MessageBox.Show("Are you sure you want to clear all queues, stacks, and records?", "Confirm Reset", MessageBoxButton.YesNo, MessageBoxImage.Question);
        if (result == MessageBoxResult.Yes)
        {
            _service.Reset();
            ShowBanner("Emergency Department records reset to empty state.", isSuccess: true);
            RefreshDashboard();
        }
    }

    private void ShowBanner(string message, bool isSuccess)
    {
        NotificationText.Text = message;
        NotificationBanner.Background = new SolidColorBrush(isSuccess ? Color.FromRgb(6, 78, 59) : Color.FromRgb(127, 29, 29));
        NotificationBanner.BorderBrush = new SolidColorBrush(isSuccess ? Color.FromRgb(16, 185, 129) : Color.FromRgb(239, 68, 68));
        NotificationText.Foreground = new SolidColorBrush(isSuccess ? Color.FromRgb(167, 243, 208) : Color.FromRgb(254, 202, 202));
        NotificationBanner.Visibility = Visibility.Visible;
    }

    private void OnDismissBannerClicked(object sender, RoutedEventArgs e)
    {
        NotificationBanner.Visibility = Visibility.Collapsed;
    }
}

public class WaitingPatientViewModel
{
    public string PositionLabel { get; set; } = string.Empty;
    public string FormattedArrivalTime { get; set; } = string.Empty;
    public int PatientNumber { get; set; }
    public string PatientName { get; set; } = string.Empty;
    public int Age { get; set; }
    public string TriageCategory { get; set; } = string.Empty;
    public string ReasonForVisit { get; set; } = string.Empty;
}

public class ActionHistoryViewModel
{
    public string StackLabel { get; set; } = string.Empty;
    public string FormattedTimestamp { get; set; } = string.Empty;
    public int ActionId { get; set; }
    public string PatientDisplay { get; set; } = string.Empty;
    public string ActionType { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string StaffName { get; set; } = string.Empty;
}