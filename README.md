# 🏥 Emergency Department Management System — C# Queue & Stack Application

[![.NET 8.0](https://img.shields.io/badge/.NET-8.0-512BD4?logo=dotnet&logoColor=white)](https://dotnet.microsoft.com/)
[![C#](https://img.shields.io/badge/C%23-12.0-239120?logo=csharp&logoColor=white)](https://learn.microsoft.com/en-us/dotnet/csharp/)
[![WPF GUI](https://img.shields.io/badge/GUI-WPF-0078D4?logo=windows&logoColor=white)](https://learn.microsoft.com/en-us/dotnet/desktop/wpf/)
[![Tests](https://img.shields.io/badge/Tests-48%20Passed%20(100%25)-brightgreen?logo=xunit)](file:///EmergencyDepartmentSystem.Tests)
[![License: MIT](https://img.shields.io/badge/License-MIT-yellow.svg)](https://opensource.org/licenses/MIT)

> **Course:** PROG7312 — Advanced Programming  
> **Activity:** ICE Task 4 — Emergency Department Management System  
> **Student Name:** Tashiel Singh  
> **Student Number:** ST10435077  
> **GitHub Submission Link:** [https://classroom.github.com/a/...](https://classroom50.org/EMKNDN/emkndn-prog7312-g2-2026/assignments/prog7312-ice-task-4/accept)  

---

## 📋 Scenario & Problem Statement

You have been asked to develop a simple **Emergency Department Management System** for a local hospital.

The emergency department receives patients throughout the day. Under normal circumstances, patients should be attended to in the **order in which they arrive**.

**Arrival Scenario Example:**
- `08:01` — Patient 1001 — John
- `08:03` — Patient 1002 — Sarah
- `08:05` — Patient 1003 — Michael
- `08:07` — Patient 1004 — Thand

*John arrived first, so John should normally be attended to first.*

However, the hospital also needs to keep track of **actions performed by medical staff on patient records**.

**Clinical Action History Example:**
1. Blood pressure updated
2. Medication added
3. Allergy information updated
4. Doctor's notes added

*If the nurse makes a mistake and selects Undo, the most recent action must be reversed first.*

---

## 🧠 Theoretical Data Structure Selection & Computer Science Analysis

The problem mandates managing two distinct operational workflows with fundamentally opposite access patterns:

### 1. Patient Waiting List ➔ `Queue<T>` (First-In, First-Out / FIFO)

- **Requirement:** Patients must be processed strictly in the order in which they arrive.
- **Optimal Data Structure:** **FIFO Queue** (`EmergencyDepartmentSystem.Core.DataStructures.PatientQueue` wrapping `System.Collections.Generic.Queue<Patient>`).
- **Algorithmic Justification:**
  - In an emergency triage setting, fairness and sequential processing demand that the earliest arrival is attended first ($O(1)$ removal from head).
  - New arrivals are registered and placed at the rear of the line ($O(1)$ insertion at tail).
  - The queue prevents starvation and guarantees strict chronological equity.
- **Time Complexities:**
  - `Enqueue(patient)`: $\mathcal{O}(1)$ amortized
  - `Dequeue()`: $\mathcal{O}(1)$
  - `Peek()`: $\mathcal{O}(1)$
  - `ContainsPatient(id)`: $\mathcal{O}(n)$

```
                     QUEUE (FIFO) ARCHITECTURE
   Arrivals (Tail)                                   Consultation (Head)
   +-------------+    +-------------+    +-------------+    +-------------+
-->| #1004 Thand |--->|#1003 Michael|--->| #1002 Sarah |--->|  #1001 John  |--> DEQUEUE (First Out)
   +-------------+    +-------------+    +-------------+    +-------------+
      [08:07]            [08:05]            [08:03]            [08:01]
```

### 2. Patient Action History ➔ `Stack<T>` (Last-In, First-Out / LIFO)

- **Requirement:** Actions performed by medical staff must support reversible operations (Undo), where the most recent action must be undone first.
- **Optimal Data Structure:** **LIFO Stack** (`EmergencyDepartmentSystem.Core.DataStructures.ActionStack` wrapping `System.Collections.Generic.Stack<PatientAction>`).
- **Algorithmic Justification:**
  - Medical and audit safety requires rollbacks to happen in reverse order of mutation.
  - The most recent state change resides directly on top of the stack.
  - Pushing an action records a new event in $\mathcal{O}(1)$ time.
  - Popping the stack undoes the latest change in $\mathcal{O}(1)$ time without disturbing earlier medical entries.
- **Time Complexities:**
  - `Push(action)`: $\mathcal{O}(1)$ amortized
  - `Pop()`: $\mathcal{O}(1)$ (Undo operation)
  - `Peek()`: $\mathcal{O}(1)$ (View most recent action)
  - `Clear()`: $\mathcal{O}(1)$

```
                    STACK (LIFO) ARCHITECTURE
                      +---------------------------------------+
  PUSH / POP (Undo) ->|  [TOP] Action #4: Doctor's notes      |  <- Most recent (Popped first)
                      +---------------------------------------+
                      |  Action #3: Allergy information       |
                      +---------------------------------------+
                      |  Action #2: Medication added          |
                      +---------------------------------------+
                      |  [BOTTOM] Action #1: Blood pressure   |  <- Oldest entry
                      +---------------------------------------+
```

---

## ✨ Functional Requirements & Implementation Matrix

| Menu Option | Requirement Description | Implementation Detail | Source File |
| :--- | :--- | :--- | :--- |
| **1. Register Patient** | Capture Patient number, Name, Age, Reason for visit, and Triage category. | Validates non-duplicate positive ID, non-empty fields, age range [0-130], and enqueues to rear of FIFO queue. | [`PatientQueue.cs`](file:///EmergencyDepartmentSystem.Core/DataStructures/PatientQueue.cs) |
| **2. View Waiting Patients** | Display all patients currently waiting in FIFO arrival order. | Enumerates queue from head to tail with positions (1st NEXT, 2nd, 3rd, ...), arrival times, age, reason. | [`Program.cs#ViewWaitingPatients`](file:///EmergencyDepartmentSystem.Cli/Program.cs) |
| **3. Call Next Patient** | Receptionist calls next patient in line. | Dequeues the head patient, marks status `InConsultation`, timestamps attendance, and logs to attended pool. | [`EmergencyDepartmentService.cs`](file:///EmergencyDepartmentSystem.Core/Services/EmergencyDepartmentService.cs) |
| **4. Record Patient Action** | Medical staff record clinical actions on a patient record. | Creates action entry with auto-increment ID, links patient name, and pushes to top of LIFO stack. | [`ActionStack.cs`](file:///EmergencyDepartmentSystem.Core/DataStructures/ActionStack.cs) |
| **5. View Most Recent Action** | Allow medical staff to inspect the most recent action. | Peeks the top of the stack in $\mathcal{O}(1)$ without removing it, displaying timestamp, action type, and staff. | [`ActionStack.cs#Peek`](file:///EmergencyDepartmentSystem.Core/DataStructures/ActionStack.cs) |
| **6. Undo Last Action** | Medical staff undo / reverse the most recent action. | Pops the topmost action from the stack in $\mathcal{O}(1)$, confirming reversal and exposing the new top. | [`ActionStack.cs#Pop`](file:///EmergencyDepartmentSystem.Core/DataStructures/ActionStack.cs) |
| **7. Display Statistics** | Display system state: total waiting, next in line, total attended, actions count, latest action, average age. | Computes summary metrics across both FIFO queue and LIFO stack. | [`EmergencyDepartmentStats.cs`](file:///EmergencyDepartmentSystem.Core/Models/EmergencyDepartmentStats.cs) |
| **8. Exit** | Graceful application shutdown. | Exits loop with confirmation message. | [`Program.cs`](file:///EmergencyDepartmentSystem.Cli/Program.cs) |
| **9. WPF GUI Dashboard** | Real-time visual interface with glassmorphic dark theme. | Interactive desktop dashboard with live KPIs, DataGrids, inline register/action cards, and banner alerts. | [`MainWindow.xaml`](file:///EmergencyDepartmentSystem.App/MainWindow.xaml) |

---

## 🏗️ Project Architecture

The solution adheres strictly to clean architecture principles with complete separation of concerns:

```
EmergencyDepartmentSystem/
├── EmergencyDepartmentSystem.sln            # Visual Studio Solution File
├── EmergencyDepartmentSystem.slnx           # Modern .NET XML Solution File
├── .gitignore                               # Standard .NET build gitignore
├── README.md                                # Comprehensive documentation
├── ST10435077 - PROG7312 - ICE Task 4.txt   # Repository submission URL file
│
├── EmergencyDepartmentSystem.Core/          # Class Library (net8.0)
│   ├── Models/
│   │   ├── Patient.cs                       # Patient domain entity with status and triage
│   │   ├── PatientAction.cs                 # Clinical action record entity
│   │   └── EmergencyDepartmentStats.cs      # Metrics and aggregated state snapshot
│   ├── DataStructures/
│   │   ├── IPatientQueue.cs                 # FIFO Queue interface
│   │   ├── PatientQueue.cs                  # Thread-safe FIFO Queue implementation
│   │   ├── IActionStack.cs                  # LIFO Stack interface
│   │   └── ActionStack.cs                   # Thread-safe LIFO Stack implementation
│   ├── Services/
│   │   ├── IEmergencyDepartmentService.cs   # Emergency department service contract
│   │   └── EmergencyDepartmentService.cs    # Orchestration service coordinating Queue and Stack
│   └── Data/
│       └── SampleEmergencyData.cs           # Pre-configured seed data from assignment prompt
│
├── EmergencyDepartmentSystem.Cli/           # Console Application (net8.0)
│   └── Program.cs                           # Menu-driven CLI with colored tables and validations
│
├── EmergencyDepartmentSystem.App/           # Desktop Application (net8.0-windows WPF)
│   ├── App.xaml / App.xaml.cs               # WPF application lifecycle
│   └── MainWindow.xaml / .cs                # Glassmorphic dark dashboard with live queues and stacks
│
└── EmergencyDepartmentSystem.Tests/         # Automated Unit Test Suite (xUnit)
    ├── PatientQueueTests.cs                 # 18 FIFO Queue behavior unit tests
    ├── ActionStackTests.cs                  # 14 LIFO Stack behavior unit tests
    ├── EmergencyDepartmentServiceTests.cs   # 11 Service integration and scenario workflow tests
    └── DomainModelTests.cs                  # 5 Model validation and formatting tests
```

---

## 🚀 Getting Started & Execution

### Prerequisites
- [.NET 8.0 SDK](https://dotnet.microsoft.com/download/dotnet/8.0) or later.
- Windows OS for WPF GUI; CLI and Core are fully cross-platform.

### 1. Run the Interactive Console CLI
```powershell
dotnet run --project EmergencyDepartmentSystem.Cli
```

### 2. Run the Modern WPF GUI Dashboard
```powershell
dotnet run --project EmergencyDepartmentSystem.App
```

### 3. Run the Automated Test Suite
```powershell
dotnet test EmergencyDepartmentSystem.sln
```

---

## 🧪 Test Suite Results

The test suite contains **48 unit tests** achieving 100% domain coverage with sub-20ms execution:

```text
Passed!  - Failed: 0, Passed: 48, Skipped: 0, Total: 48, Duration: 18 ms - EmergencyDepartmentSystem.Tests.dll
```

---

## 👤 Author

- **Student:** Tashiel Singh  
- **Student Number:** ST10435077  
- **Institution:** The Independent Institute of Education (IIE) / Varsity College  
- **Course:** PROG7312 — Advanced Programming  
- **Task:** ICE Task 4  

---
Licensed under the [MIT License](LICENSE).