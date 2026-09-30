using System.Diagnostics;
using EmergencyDepartmentSystem.Core.Models;
using EmergencyDepartmentSystem.Core.Services;

namespace EmergencyDepartmentSystem.Cli;

internal class Program
{
    private static readonly IEmergencyDepartmentService _service = new EmergencyDepartmentService();

    private static void Main(string[] args)
    {
        Console.OutputEncoding = System.Text.Encoding.UTF8;
        try { Console.Title = "Emergency Department Management System - PROG7312 ICE 4"; } catch { }

        // Seed initial assignment scenario data so the user can immediately test
        _service.SeedSampleData();

        bool exitRequested = false;
        while (!exitRequested)
        {
            PrintHeader();
            PrintMenu();
            Console.ForegroundColor = ConsoleColor.Cyan;
            Console.Write("Please select an option [1-8] (or 9 for GUI): ");
            Console.ResetColor();

            string? choice = Console.ReadLine()?.Trim();
            Console.WriteLine();

            switch (choice)
            {
                case "1":
                    RegisterPatient();
                    break;
                case "2":
                    ViewWaitingPatients();
                    break;
                case "3":
                    CallNextPatient();
                    break;
                case "4":
                    RecordPatientAction();
                    break;
                case "5":
                    ViewMostRecentAction();
                    break;
                case "6":
                    UndoLastAction();
                    break;
                case "7":
                    DisplayStatistics();
                    break;
                case "8":
                    exitRequested = true;
                    Console.ForegroundColor = ConsoleColor.Green;
                    Console.WriteLine("Exiting Emergency Department Management System. Goodbye!");
                    Console.ResetColor();
                    break;
                case "9":
                    LaunchWpfGui();
                    break;
                case "10":
                case "seed":
                    _service.SeedSampleData();
                    Console.ForegroundColor = ConsoleColor.Green;
                    Console.WriteLine("Scenario sample data reloaded (John, Sarah, Michael, Thand + 4 actions).");
                    Console.ResetColor();
                    Pause();
                    break;
                default:
                    Console.ForegroundColor = ConsoleColor.Red;
                    Console.WriteLine("Invalid option. Please enter a valid number (1 to 8).");
                    Console.ResetColor();
                    Pause();
                    break;
            }
        }
    }

    private static void PrintHeader()
    {
        try { Console.Clear(); } catch { }
        Console.ForegroundColor = ConsoleColor.DarkCyan;
        Console.WriteLine("================================================================================");
        Console.ForegroundColor = ConsoleColor.Cyan;
        Console.WriteLine("                    EMERGENCY DEPARTMENT MANAGEMENT SYSTEM                      ");
        Console.ForegroundColor = ConsoleColor.DarkGray;
        Console.WriteLine("           PROG7312 Advanced Programming | ICE Task 4 | ST10435077              ");
        Console.ForegroundColor = ConsoleColor.DarkCyan;
        Console.WriteLine("================================================================================");
        Console.ResetColor();

        var stats = _service.GetStatistics();
        Console.ForegroundColor = ConsoleColor.Yellow;
        var nextPatientText = stats.NextPatientInQueue != null 
            ? $"Next in Line: #{stats.NextPatientInQueue.PatientNumber} ({stats.NextPatientInQueue.PatientName})"
            : "Next in Line: None (Queue Empty)";
        Console.WriteLine($"[QUEUE (FIFO)]: {stats.TotalWaiting} Waiting | {nextPatientText}");
        Console.ForegroundColor = ConsoleColor.Magenta;
        var lastActionText = stats.MostRecentAction != null 
            ? $"Top of Stack: #{stats.MostRecentAction.ActionId} ({stats.MostRecentAction.ActionType})"
            : "Top of Stack: Empty";
        Console.WriteLine($"[STACK (LIFO)]: {stats.TotalActionsRecorded} Actions | {lastActionText}");
        Console.ResetColor();
        Console.WriteLine("--------------------------------------------------------------------------------");
    }

    private static void PrintMenu()
    {
        Console.ForegroundColor = ConsoleColor.White;
        Console.WriteLine("EMERGENCY DEPARTMENT SYSTEM");
        Console.ForegroundColor = ConsoleColor.Gray;
        Console.WriteLine("  1. Register Patient");
        Console.WriteLine("  2. View Waiting Patients");
        Console.WriteLine("  3. Call Next Patient");
        Console.WriteLine("  4. Record Patient Action");
        Console.WriteLine("  5. View Most Recent Action");
        Console.WriteLine("  6. Undo Last Action");
        Console.WriteLine("  7. Display Statistics");
        Console.WriteLine("  8. Exit");
        Console.ForegroundColor = ConsoleColor.DarkGray;
        Console.WriteLine("  [9. Launch Modern WPF GUI Dashboard | 10. Reload Scenario Seed Data]");
        Console.ResetColor();
        Console.WriteLine("--------------------------------------------------------------------------------");
    }

    private static void RegisterPatient()
    {
        Console.ForegroundColor = ConsoleColor.Cyan;
        Console.WriteLine(">>> 1. REGISTER PATIENT (Enqueues into FIFO Waiting Queue)");
        Console.ResetColor();
        Console.WriteLine("Capture patient details to add them to the arrival waiting line.");
        Console.WriteLine();

        int patientNumber;
        while (true)
        {
            Console.Write("Enter Patient Number (e.g. 1005): ");
            string? input = Console.ReadLine()?.Trim();
            if (int.TryParse(input, out patientNumber) && patientNumber > 0)
            {
                if (_service.WaitingQueue.ContainsPatient(patientNumber))
                {
                    Console.ForegroundColor = ConsoleColor.Yellow;
                    Console.WriteLine($"Patient #{patientNumber} is already in the waiting queue. Please use a unique number.");
                    Console.ResetColor();
                    continue;
                }
                break;
            }
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine("Invalid patient number. Please enter a positive integer.");
            Console.ResetColor();
        }

        string patientName;
        while (true)
        {
            Console.Write("Enter Patient Name: ");
            patientName = Console.ReadLine()?.Trim() ?? string.Empty;
            if (!string.IsNullOrWhiteSpace(patientName))
                break;
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine("Patient name cannot be empty.");
            Console.ResetColor();
        }

        int age;
        while (true)
        {
            Console.Write("Enter Patient Age: ");
            string? input = Console.ReadLine()?.Trim();
            if (int.TryParse(input, out age) && age >= 0 && age <= 130)
                break;
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine("Invalid age. Please enter an age between 0 and 130.");
            Console.ResetColor();
        }

        string reason;
        while (true)
        {
            Console.Write("Enter Reason for Visit: ");
            reason = Console.ReadLine()?.Trim() ?? string.Empty;
            if (!string.IsNullOrWhiteSpace(reason))
                break;
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine("Reason for visit cannot be empty.");
            Console.ResetColor();
        }

        Console.WriteLine();
        Console.WriteLine("Select Triage Category:");
        Console.WriteLine("  1. Standard");
        Console.WriteLine("  2. Moderate");
        Console.WriteLine("  3. Urgent / Priority");
        Console.Write("Choice [1-3, default=1]: ");
        string triageChoice = Console.ReadLine()?.Trim() ?? "1";
        string triage = triageChoice switch
        {
            "2" => "Priority 2 - Moderate",
            "3" => "Priority 1 - Urgent",
            _ => "Priority 3 - Standard"
        };

        try
        {
            var registered = _service.RegisterPatient(patientNumber, patientName, age, reason, DateTime.Now, triage);
            Console.WriteLine();
            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine("[SUCCESS] Patient Registered and Enqueued!");
            Console.WriteLine($"  Arrival Time   : {registered.FormattedArrivalTime}");
            Console.WriteLine($"  Patient Number : {registered.PatientNumber}");
            Console.WriteLine($"  Name & Age     : {registered.PatientName} (Age: {registered.Age})");
            Console.WriteLine($"  Reason         : {registered.ReasonForVisit}");
            Console.WriteLine($"  Triage Level   : {registered.TriageCategory}");
            Console.WriteLine($"  Queue Position : #{_service.WaitingQueue.Count} in line (FIFO)");
            Console.ResetColor();
        }
        catch (Exception ex)
        {
            Console.WriteLine();
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine($"[ERROR] Registration failed: {ex.Message}");
            Console.ResetColor();
        }

        Pause();
    }

    private static void ViewWaitingPatients()
    {
        Console.ForegroundColor = ConsoleColor.Cyan;
        Console.WriteLine(">>> 2. VIEW WAITING PATIENTS (FIFO Queue Order)");
        Console.ResetColor();
        Console.WriteLine("Patients are displayed in the exact order of their arrival (Head -> Tail).");
        Console.WriteLine();

        var patients = _service.GetWaitingPatients();
        if (patients.Count == 0)
        {
            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.WriteLine("No patients are currently waiting in the emergency queue.");
            Console.ResetColor();
            Pause();
            return;
        }

        Console.ForegroundColor = ConsoleColor.DarkCyan;
        Console.WriteLine("---------------------------------------------------------------------------------------------------------");
        Console.WriteLine(string.Format("{0,-6} | {1,-8} | {2,-8} | {3,-18} | {4,-5} | {5,-20} | {6,-16}",
            "Pos", "Arrival", "Pat #", "Name", "Age", "Triage", "Reason for Visit"));
        Console.WriteLine("---------------------------------------------------------------------------------------------------------");
        Console.ResetColor();

        int pos = 1;
        foreach (var p in patients)
        {
            string posLabel = pos == 1 ? "1 (NEXT)" : pos.ToString();
            if (pos == 1) Console.ForegroundColor = ConsoleColor.Green;
            else Console.ForegroundColor = ConsoleColor.White;

            string reasonSnippet = p.ReasonForVisit.Length > 24 ? p.ReasonForVisit.Substring(0, 21) + "..." : p.ReasonForVisit;
            Console.WriteLine(string.Format("{0,-6} | {1,-8} | {2,-8} | {3,-18} | {4,-5} | {5,-20} | {6,-16}",
                posLabel, p.FormattedArrivalTime, p.PatientNumber, p.PatientName, p.Age, p.TriageCategory, reasonSnippet));
            pos++;
        }

        Console.ForegroundColor = ConsoleColor.DarkCyan;
        Console.WriteLine("---------------------------------------------------------------------------------------------------------");
        Console.ResetColor();
        Console.WriteLine($"Total patients currently in queue: {patients.Count}");
        Pause();
    }

    private static void CallNextPatient()
    {
        Console.ForegroundColor = ConsoleColor.Cyan;
        Console.WriteLine(">>> 3. CALL NEXT PATIENT (FIFO Dequeue)");
        Console.ResetColor();
        Console.WriteLine("Removes and attends to the patient at the head of the waiting queue.");
        Console.WriteLine();

        try
        {
            var patient = _service.CallNextPatient();

            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine("================================================================================");
            Console.WriteLine("           >>> CALLING PATIENT TO CONSULTATION ROOM 1 <<<                       ");
            Console.WriteLine("================================================================================");
            Console.WriteLine($"  Patient Number : {patient.PatientNumber}");
            Console.WriteLine($"  Patient Name   : {patient.PatientName}");
            Console.WriteLine($"  Age            : {patient.Age}");
            Console.WriteLine($"  Arrival Time   : {patient.FormattedArrivalTime}");
            Console.WriteLine($"  Call Time      : {DateTime.Now:HH:mm:ss}");
            Console.WriteLine($"  Reason         : {patient.ReasonForVisit}");
            Console.WriteLine($"  Triage         : {patient.TriageCategory}");
            Console.WriteLine("================================================================================");
            Console.ResetColor();

            Console.WriteLine($"Remaining patients in waiting queue: {_service.WaitingQueue.Count}");
        }
        catch (InvalidOperationException ex)
        {
            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.WriteLine($"[NOTICE] {ex.Message}");
            Console.ResetColor();
        }
        catch (Exception ex)
        {
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine($"[ERROR] {ex.Message}");
            Console.ResetColor();
        }

        Pause();
    }

    private static void RecordPatientAction()
    {
        Console.ForegroundColor = ConsoleColor.Cyan;
        Console.WriteLine(">>> 4. RECORD PATIENT ACTION (LIFO Stack Push)");
        Console.ResetColor();
        Console.WriteLine("Records a clinical action on a patient record, pushed to the top of the history stack.");
        Console.WriteLine();

        int patientNumber;
        while (true)
        {
            Console.Write("Enter Patient Number: ");
            string? input = Console.ReadLine()?.Trim();
            if (int.TryParse(input, out patientNumber) && patientNumber > 0)
                break;
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine("Please enter a valid positive patient number.");
            Console.ResetColor();
        }

        var existing = _service.FindPatient(patientNumber);
        if (existing != null)
        {
            Console.ForegroundColor = ConsoleColor.DarkGreen;
            Console.WriteLine($"Identified Patient: #{existing.PatientNumber} - {existing.PatientName} (Status: {existing.Status})");
            Console.ResetColor();
        }

        Console.WriteLine();
        Console.WriteLine("Select Action Category:");
        Console.WriteLine("  1. Blood pressure updated");
        Console.WriteLine("  2. Medication added");
        Console.WriteLine("  3. Allergy information updated");
        Console.WriteLine("  4. Doctor's notes added");
        Console.WriteLine("  5. Custom Clinical Action");
        Console.Write("Choose [1-5]: ");
        string catChoice = Console.ReadLine()?.Trim() ?? "5";

        string actionType = catChoice switch
        {
            "1" => "Blood pressure updated",
            "2" => "Medication added",
            "3" => "Allergy information updated",
            "4" => "Doctor's notes added",
            _ => "Clinical Action"
        };

        if (catChoice == "5")
        {
            Console.Write("Enter custom action type: ");
            string custom = Console.ReadLine()?.Trim() ?? string.Empty;
            if (!string.IsNullOrWhiteSpace(custom))
                actionType = custom;
        }

        string description;
        while (true)
        {
            Console.Write($"Enter details/notes for '{actionType}': ");
            description = Console.ReadLine()?.Trim() ?? string.Empty;
            if (!string.IsNullOrWhiteSpace(description))
                break;
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine("Description cannot be empty.");
            Console.ResetColor();
        }

        Console.Write("Enter Staff Member Name (default: Medical Staff): ");
        string staff = Console.ReadLine()?.Trim() ?? string.Empty;
        if (string.IsNullOrWhiteSpace(staff))
            staff = "Medical Staff";

        try
        {
            var action = _service.RecordPatientAction(patientNumber, actionType, description, staff);
            Console.WriteLine();
            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine("[SUCCESS] Action Recorded & Pushed to Top of History Stack!");
            Console.WriteLine($"  Action ID   : #{action.ActionId}");
            Console.WriteLine($"  Timestamp   : {action.FormattedTimestamp}");
            Console.WriteLine($"  Patient     : #{action.PatientNumber} ({action.PatientName})");
            Console.WriteLine($"  Action Type : {action.ActionType}");
            Console.WriteLine($"  Description : {action.Description}");
            Console.WriteLine($"  Staff       : {action.StaffName}");
            Console.WriteLine($"  Stack Depth : {_service.ActionHistory.Count} action(s) stored (LIFO)");
            Console.ResetColor();
        }
        catch (Exception ex)
        {
            Console.WriteLine();
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine($"[ERROR] Could not record action: {ex.Message}");
            Console.ResetColor();
        }

        Pause();
    }

    private static void ViewMostRecentAction()
    {
        Console.ForegroundColor = ConsoleColor.Cyan;
        Console.WriteLine(">>> 5. VIEW MOST RECENT ACTION (LIFO Stack Peek)");
        Console.ResetColor();
        Console.WriteLine("Inspects the topmost action on the stack without removing or modifying it.");
        Console.WriteLine();

        var action = _service.ViewMostRecentAction();
        if (action == null)
        {
            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.WriteLine("No actions have been recorded yet in the history stack.");
            Console.ResetColor();
            Pause();
            return;
        }

        Console.ForegroundColor = ConsoleColor.Magenta;
        Console.WriteLine("================================================================================");
        Console.WriteLine("                >>> MOST RECENT ACTION (TOP OF STACK) <<<                       ");
        Console.WriteLine("================================================================================");
        Console.ResetColor();
        Console.WriteLine($"  Action ID   : #{action.ActionId}");
        Console.WriteLine($"  Timestamp   : {action.FormattedTimestamp}");
        Console.WriteLine($"  Patient     : #{action.PatientNumber} ({action.PatientName})");
        Console.WriteLine($"  Action Type : {action.ActionType}");
        Console.WriteLine($"  Description : {action.Description}");
        Console.WriteLine($"  Staff       : {action.StaffName}");
        Console.ForegroundColor = ConsoleColor.DarkGray;
        Console.WriteLine("--------------------------------------------------------------------------------");
        Console.WriteLine("Note: In LIFO order, selecting 'Undo Last Action' will reverse THIS action first.");
        Console.ResetColor();

        Pause();
    }

    private static void UndoLastAction()
    {
        Console.ForegroundColor = ConsoleColor.Cyan;
        Console.WriteLine(">>> 6. UNDO LAST ACTION (LIFO Stack Pop)");
        Console.ResetColor();
        Console.WriteLine("Reverses the most recent clinical action from the top of the stack.");
        Console.WriteLine();

        try
        {
            var undone = _service.UndoLastAction();
            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.WriteLine("================================================================================");
            Console.WriteLine("                 >>> ACTION SUCCESSFULLY UNDONE / REVERSED <<<                  ");
            Console.WriteLine("================================================================================");
            Console.ResetColor();
            Console.WriteLine($"  Reversed Action ID : #{undone.ActionId}");
            Console.WriteLine($"  Original Time      : {undone.FormattedTimestamp}");
            Console.WriteLine($"  Patient            : #{undone.PatientNumber} ({undone.PatientName})");
            Console.WriteLine($"  Action Type        : {undone.ActionType}");
            Console.WriteLine($"  Description        : {undone.Description}");
            Console.WriteLine($"  Staff              : {undone.StaffName}");
            Console.ForegroundColor = ConsoleColor.DarkYellow;
            Console.WriteLine("================================================================================");
            Console.ResetColor();

            var newTop = _service.ViewMostRecentAction();
            if (newTop != null)
            {
                Console.ForegroundColor = ConsoleColor.Cyan;
                Console.WriteLine($"New top of stack is Action #{newTop.ActionId} ({newTop.ActionType}: {newTop.Description}).");
                Console.ResetColor();
            }
            else
            {
                Console.ForegroundColor = ConsoleColor.DarkGray;
                Console.WriteLine("The action history stack is now empty.");
                Console.ResetColor();
            }
        }
        catch (InvalidOperationException ex)
        {
            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.WriteLine($"[NOTICE] {ex.Message}");
            Console.ResetColor();
        }
        catch (Exception ex)
        {
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine($"[ERROR] {ex.Message}");
            Console.ResetColor();
        }

        Pause();
    }

    private static void DisplayStatistics()
    {
        Console.ForegroundColor = ConsoleColor.Cyan;
        Console.WriteLine(">>> 7. DISPLAY STATISTICS (System State & Data Structure Metrics)");
        Console.ResetColor();
        Console.WriteLine("Current operational state of queues, stacks, and patient metrics.");
        Console.WriteLine();

        var stats = _service.GetStatistics();
        var attended = _service.GetAttendedPatients();

        Console.ForegroundColor = ConsoleColor.DarkCyan;
        Console.WriteLine("================================================================================");
        Console.ForegroundColor = ConsoleColor.White;
        Console.WriteLine("                      EMERGENCY DEPARTMENT METRICS                              ");
        Console.ForegroundColor = ConsoleColor.DarkCyan;
        Console.WriteLine("================================================================================");
        Console.ResetColor();

        Console.WriteLine($"  System Timestamp            : {stats.FormattedGeneratedTime}");
        Console.WriteLine($"  Patients Currently Waiting  : {stats.TotalWaiting} (in FIFO Queue)");
        Console.WriteLine($"  Patients Attended Today     : {stats.TotalAttended}");
        Console.WriteLine($"  Clinical Actions Recorded   : {stats.TotalActionsRecorded} (in LIFO Stack)");
        Console.WriteLine($"  Average Age of Waiting      : {(stats.TotalWaiting > 0 ? stats.AverageAgeOfWaiting + " years" : "N/A")}");

        if (stats.LongestCurrentWaitTime.HasValue)
        {
            var wait = stats.LongestCurrentWaitTime.Value;
            Console.WriteLine($"  Longest Current Wait Time   : {wait.Hours:D2}h {wait.Minutes:D2}m {wait.Seconds:D2}s");
        }

        Console.ForegroundColor = ConsoleColor.Yellow;
        Console.WriteLine();
        Console.WriteLine("--- Next Patient to be Attended (Queue Head / Peek) ---");
        if (stats.NextPatientInQueue != null)
        {
            Console.WriteLine($"  #{stats.NextPatientInQueue.PatientNumber} - {stats.NextPatientInQueue.PatientName} (Age: {stats.NextPatientInQueue.Age})");
            Console.WriteLine($"  Arrival Time : {stats.NextPatientInQueue.FormattedArrivalTime} | Reason: {stats.NextPatientInQueue.ReasonForVisit}");
        }
        else
        {
            Console.WriteLine("  None (Queue is currently empty)");
        }
        Console.ResetColor();

        Console.ForegroundColor = ConsoleColor.Magenta;
        Console.WriteLine();
        Console.WriteLine("--- Most Recent Clinical Action (Stack Top / Peek) ---");
        if (stats.MostRecentAction != null)
        {
            Console.WriteLine($"  Action #{stats.MostRecentAction.ActionId} | [{stats.MostRecentAction.FormattedTimestamp}]");
            Console.WriteLine($"  Patient #{stats.MostRecentAction.PatientNumber} ({stats.MostRecentAction.PatientName})");
            Console.WriteLine($"  {stats.MostRecentAction.ActionType}: {stats.MostRecentAction.Description}");
        }
        else
        {
            Console.WriteLine("  None (Action history stack is currently empty)");
        }
        Console.ResetColor();

        if (attended.Count > 0)
        {
            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine();
            Console.WriteLine("--- Patients Attended / In Consultation Today ---");
            foreach (var att in attended)
            {
                Console.WriteLine($"  * #{att.PatientNumber} - {att.PatientName} (Attended at: {att.AttendedTime:HH:mm:ss})");
            }
            Console.ResetColor();
        }

        Console.ForegroundColor = ConsoleColor.DarkCyan;
        Console.WriteLine("================================================================================");
        Console.ResetColor();

        Pause();
    }

    private static void LaunchWpfGui()
    {
        Console.ForegroundColor = ConsoleColor.Cyan;
        Console.WriteLine("Launching Modern WPF GUI Dashboard...");
        Console.ResetColor();

        try
        {
            string appExe = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "..", "..", "..", "..", "EmergencyDepartmentSystem.App", "bin", "Debug", "net8.0-windows", "EmergencyDepartmentSystem.App.exe");
            if (File.Exists(appExe))
            {
                Process.Start(new ProcessStartInfo(appExe) { UseShellExecute = true });
                Console.ForegroundColor = ConsoleColor.Green;
                Console.WriteLine("WPF GUI launched successfully in separate window.");
                Console.ResetColor();
            }
            else
            {
                Console.ForegroundColor = ConsoleColor.Yellow;
                Console.WriteLine("WPF GUI build binary not found yet. Running 'dotnet run --project EmergencyDepartmentSystem.App'...");
                Console.ResetColor();
                Process.Start(new ProcessStartInfo("dotnet", "run --project EmergencyDepartmentSystem.App") 
                { 
                    WorkingDirectory = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "..", "..", "..", ".."),
                    UseShellExecute = true 
                });
            }
        }
        catch (Exception ex)
        {
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine($"Could not launch WPF GUI: {ex.Message}");
            Console.ResetColor();
        }

        Pause();
    }

    private static void Pause()
    {
        Console.WriteLine();
        Console.ForegroundColor = ConsoleColor.DarkGray;
        Console.Write("Press [Enter] to continue...");
        Console.ResetColor();
        Console.ReadLine();
    }
}