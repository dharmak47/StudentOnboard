# 📋 Unit Test Case Report
## Project: Student Onboarding Platform
**Prepared By:** Malini V (vmalini581@gmail.com)
**Date:** 08 July 2026
**Framework:** xUnit v2.9.3 + Moq v4.20.72
**Language:** C# / .NET 8.0
**Test Type:** Unit Testing (Mock-based)
**Tool Used:** Visual Studio 2022 / dotnet test CLI

---

## ✅ Final Test Execution Summary

| Item | Details |
|------|---------|
| **Total Test Cases Written** | 34 |
| **Total Passed** | **34** |
| **Total Failed** | **0** |
| **Total Skipped** | 0 |
| **Execution Time** | 2.0 seconds |
| **Build Status** | ✅ Succeeded |
| **Test Runner** | xUnit.net VSTest Adapter v3.1.4 |
| **Target Framework** | .NET 8.0 |

---

> ### 📸 Terminal Proof (Screenshot Evidence)
> **Command run:**
> ```
> dotnet test --verbosity normal
> ```
> **Output:**
> ```
> Test summary: total: 34, failed: 0, succeeded: 34, skipped: 0, duration: 2.0s
> Build succeeded with 2 warning(s) in 5.7s
> ```
> *(Screenshot captured from Visual Studio 2022 Developer PowerShell — showing 34/34 tests passing)*

---

## 📌 What Type of Testing Was Done?

### Unit Testing
Unit Testing tests **individual service methods in isolation** using **mock objects** (simulated dependencies). This means:
- No real database is connected during tests
- No real email is sent during tests
- Each function is tested independently with controlled inputs and expected outputs
- This proves the **business logic** works correctly

---

## 📂 Test Cases — Module Wise

---

### Module 1: Authentication Service (`AuthService`)
**File:** `AuthServiceTests.cs`
**Purpose:** Tests user login logic

| TC # | Test Case Name | Test Input | Expected Output | Actual Output | Result |
|------|---------------|------------|-----------------|---------------|--------|
| TC-01 | Login with non-existent user email | Email: `nonexistent@example.com` | Failure: "Invalid email or password." | Failure: "Invalid email or password." | ✅ PASS |
| TC-02 | Login with wrong password | Email: valid, Password: wrong | Failure: "Invalid email or password." | Failure: "Invalid email or password." | ✅ PASS |
| TC-03 | Login with inactive/deactivated account | `IsActive = false` | Failure: "Your account is inactive." | Failure: "Your account is inactive." | ✅ PASS |
| TC-04 | Login with valid credentials | Correct email + password | Success + JWT Token returned | Success + Access Token + Refresh Token | ✅ PASS |

---

### Module 2: User Service (`UserService`)
**File:** `UserServiceTests.cs`
**Purpose:** Tests user data operations

| TC # | Test Case Name | Test Input | Expected Output | Actual Output | Result |
|------|---------------|------------|-----------------|---------------|--------|
| TC-05 | Get user by valid email | Email: `user@test.com` | Returns user object | Returns user with correct email | ✅ PASS |
| TC-06 | Create a new user and save to database | New user object | User saved, repository called once | Repository CreateAsync called once | ✅ PASS |
| TC-07 | Update last login timestamp | UserId | Repository UpdateLastLogin called | Repository method called with correct UserId | ✅ PASS |

---

### Module 3: Course Service (`CourseService`)
**File:** `CourseServiceTests.cs`
**Purpose:** Tests course CRUD operations

| TC # | Test Case Name | Test Input | Expected Output | Actual Output | Result |
|------|---------------|------------|-----------------|---------------|--------|
| TC-08 | Get all active courses | — | Returns list of 3 courses | List with React, Java, .NET returned | ✅ PASS |
| TC-09 | Get active courses when no courses exist | — | Returns empty list | Empty list returned | ✅ PASS |
| TC-10 | Get course by valid ID | Valid Course GUID | Returns course details | Course with correct name returned | ✅ PASS |
| TC-11 | Get course by invalid/non-existent ID | Random GUID | Failure: "Course not found." | Failure: "Course not found." | ✅ PASS |
| TC-12 | Create a new course with valid data | Course name, fees, category | Success: course created | Success + course details returned | ✅ PASS |
| TC-13 | Verify new course IsActive is set to true | New course request | `IsActive = true`, `IsDeleted = false` | Captured course has IsActive=true | ✅ PASS |
| TC-14 | Update an existing course | Updated name, fees | Success: course updated | Updated name returned in response | ✅ PASS |
| TC-15 | Update a non-existent course | Invalid Course GUID | Failure: "Course not found." | Failure: "Course not found." | ✅ PASS |
| TC-16 | Delete course with no enrolled students | Course with 0 registrations | Success: course deleted | SoftDelete called, success returned | ✅ PASS |
| TC-17 | Delete course that has enrolled students | Course with 5 students | Failure: "5 active student(s) enrolled" | Failure with student count message | ✅ PASS |
| TC-18 | Delete non-existent course | Invalid Course GUID | Failure: "Course not found." | Failure: "Course not found." | ✅ PASS |

---

### Module 4: Student Service (`StudentService`)
**File:** `StudentServiceTests.cs`
**Purpose:** Tests student profile operations

| TC # | Test Case Name | Test Input | Expected Output | Actual Output | Result |
|------|---------------|------------|-----------------|---------------|--------|
| TC-19 | Get student profile with valid user ID | Valid UserId | Returns profile (name, email) | Profile with "Malini", "vmalini581@gmail.com" | ✅ PASS |
| TC-20 | Get student profile with invalid user ID | Random GUID | Failure: "User not found." | Failure: "User not found." | ✅ PASS |
| TC-21 | Update student profile with valid data | New name, phone, address | Success + updated profile | "Profile updated successfully." | ✅ PASS |
| TC-22 | Update profile for non-existent user | Random UserId | Failure: "User not found." | Failure: "User not found." | ✅ PASS |
| TC-23 | Upload profile photo for valid user | Image file mock | Photo URL saved, success message | URL saved, "Photo uploaded successfully." | ✅ PASS |
| TC-24 | Upload profile photo for non-existent user | Random UserId + file | Failure: "User not found." | Failure: no upload, "User not found." | ✅ PASS |

---

### Module 5: Admin Service (`AdminService`)
**File:** `AdminServiceTests.cs`
**Purpose:** Tests admin operations — dashboard, student management

| TC # | Test Case Name | Test Input | Expected Output | Actual Output | Result |
|------|---------------|------------|-----------------|---------------|--------|
| TC-25 | Get dashboard with real data | 10 students, 3 courses, 8 registrations | Returns all correct counts | TotalStudents=10, Pending=3, Approved=5, Courses=3 | ✅ PASS |
| TC-26 | Get dashboard when no data exists | 0 students, 0 courses | All counts = 0 | TotalStudents=0, TotalCourses=0 | ✅ PASS |
| TC-27 | Get paginated list of all students | Page 1, PageSize 10 | Returns 2 students, TotalCount=2 | 2 students returned with pagination | ✅ PASS |
| TC-28 | Get students filtered by Approved status | Status = "Approved" | Returns only Approved students | 1 Approved student returned | ✅ PASS |
| TC-29 | Approve a pending student | StudentId + AdminId | Success + email sent + notification | "Student approved successfully." + email called | ✅ PASS |
| TC-30 | Approve non-existent student | Random StudentId | Failure: "Student not found." | Failure: "Student not found." | ✅ PASS |
| TC-31 | Approve already approved student | Student with `ApprovalStatus = Approved` | Failure: "already approved" | Failure: "Student is already approved." | ✅ PASS |
| TC-32 | Deny a pending student with reason | StudentId + Reason | Success + denial email + session revoked | "Student denied successfully." + email called | ✅ PASS |
| TC-33 | Deny non-existent student | Random StudentId | Failure: "Student not found." | Failure: "Student not found." | ✅ PASS |
| TC-34 | Deny already denied student | Student with `ApprovalStatus = Denied` | Failure: "already denied" | Failure: "Student is already denied." | ✅ PASS |

---

## 📊 Module-wise Test Results

| Module | Total TCs | Passed | Failed |
|--------|-----------|--------|--------|
| AuthService | 4 | 4 | 0 |
| UserService | 3 | 3 | 0 |
| CourseService | 11 | 11 | 0 |
| StudentService | 6 | 6 | 0 |
| AdminService | 10 | 10 | 0 |
| **TOTAL** | **34** | **34** | **0** |

---

## 🧰 Tools & Technologies Used

| Tool | Purpose |
|------|---------|
| **xUnit v2.9.3** | Unit test framework |
| **Moq v4.20.72** | Mock object library (for simulating DB, email, services) |
| **Microsoft.NET.Test.Sdk** | Test execution engine |
| **coverlet.collector** | Code coverage collection |
| **Visual Studio 2022** | IDE for development and debugging |
| **dotnet test CLI** | Command-line test runner |

---

## 📂 Test Project File Locations

```
StudentOnboard-Repo/
└── Student_Onboarding_Backend/
    └── Student_Onboarding_Backend/
        └── StudentOnboarding.Tests/
            ├── AuthServiceTests.cs       → TC-01 to TC-04
            ├── UserServiceTests.cs       → TC-05 to TC-07
            ├── CourseServiceTests.cs     → TC-08 to TC-18
            ├── StudentServiceTests.cs    → TC-19 to TC-24
            └── AdminServiceTests.cs     → TC-25 to TC-34
```

---

## ▶ How to Re-Run Tests (Reproducibility)

```powershell
cd "C:\Users\HP\Desktop\Downloads\what\StudentOnboard-Repo\Student_Onboarding_Backend\Student_Onboarding_Backend\StudentOnboarding.Tests"

dotnet test --verbosity normal
```

**Expected output:**
```
Test summary: total: 34, failed: 0, succeeded: 34, skipped: 0, duration: 2.0s
Build succeeded with 2 warning(s) in 5.7s
```

---

## ✍ Sign-Off

| Role | Name | Date |
|------|------|------|
| **Developer / Tester** | Malini V | 08-July-2026 |
| **Reviewed By** | *(Manager Name)* | |
| **Status** | ✅ All Tests Passed — Ready for Review | |

---
*This report was generated from actual test execution using `dotnet test` on the StudentOnboarding Platform project.*
