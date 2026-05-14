# Student Onboarding Platform -- Phase 2 API Reference

> **Version:** 2.0
> **Base URL:** `https://<your-domain>` (development: `https://localhost:<port>`)
> **Content-Type:** `application/json` for all requests and responses
> **Authentication:** JWT Bearer tokens via the `Authorization` header

---

## Table of Contents

1. [Response Envelope](#response-envelope)
2. [Pagination Envelope](#pagination-envelope)
3. [Enum Reference](#enum-reference)
4. [Authentication Headers](#authentication-headers)
5. [Auth Endpoints](#1-auth-endpoints)
6. [Student Endpoints](#2-student-endpoints)
7. [Admin Endpoints](#3-admin-endpoints)
8. [Course Endpoints](#4-course-endpoints)
9. [Error Handling](#error-handling)

---

## Response Envelope

Every response from the API is wrapped in a standard `ApiResponse<T>` envelope.

```json
{
  "success": true,
  "message": "Success",
  "data": { },
  "errors": null
}
```

| Field     | Type       | Description                                         |
|-----------|------------|-----------------------------------------------------|
| `success` | `boolean`  | `true` when the operation succeeded                 |
| `message` | `string`   | Human-readable status message                       |
| `data`    | `T \| null`| The response payload (type varies per endpoint)     |
| `errors`  | `string[] \| null` | List of validation or business-rule errors  |

On failure the shape is:

```json
{
  "success": false,
  "message": "Validation failed",
  "data": null,
  "errors": [
    "Email is required.",
    "Password must be at least 8 characters."
  ]
}
```

---

## Pagination Envelope

Paginated endpoints return `ApiResponse<PaginatedResponse<T>>`. The `data` field contains:

```json
{
  "items": [ ],
  "totalCount": 42,
  "page": 1,
  "pageSize": 10,
  "totalPages": 5,
  "hasNextPage": true,
  "hasPreviousPage": false
}
```

| Field             | Type      | Description                                    |
|-------------------|-----------|------------------------------------------------|
| `items`           | `T[]`     | Array of results for the current page          |
| `totalCount`      | `integer` | Total number of records matching the query     |
| `page`            | `integer` | Current page number (1-based)                  |
| `pageSize`        | `integer` | Number of items per page                       |
| `totalPages`      | `integer` | Calculated total pages                         |
| `hasNextPage`     | `boolean` | `true` if there is a page after the current    |
| `hasPreviousPage` | `boolean` | `true` if there is a page before the current   |

---

## Enum Reference

### ApprovalStatus
| Value      | Description                          |
|------------|--------------------------------------|
| `Pending`  | Student awaiting admin review        |
| `Approved` | Student approved by admin            |
| `Denied`   | Student denied by admin              |

### PaymentStatus
| Value      | Description                          |
|------------|--------------------------------------|
| `Pending`  | Payment not yet received             |
| `Paid`     | Full payment received                |
| `Partial`  | Partial payment received             |
| `Refunded` | Payment has been refunded            |

### NotificationType
| Value                | Description                             |
|----------------------|-----------------------------------------|
| `NewRegistration`    | A new student signed up                 |
| `CourseRegistration`  | A student registered for a course       |
| `StudentApproved`    | A student was approved by admin         |
| `StudentDenied`      | A student was denied by admin           |

### UserRole
| Value     | Description           |
|-----------|-----------------------|
| `Student` | Regular student user  |
| `Admin`   | Platform administrator|

---

## Authentication Headers

All endpoints marked **[Auth Required]** require a valid JWT access token:

```
Authorization: Bearer eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9...
```

Endpoints marked **[Admin]** additionally require the authenticated user to have the `Admin` role. If a non-admin user calls an admin endpoint, the API returns `403 Forbidden`.

If no token is supplied or the token is expired, the API returns `401 Unauthorized`.

---

## 1. Auth Endpoints

**Base path:** `/api/auth`
**Controller:** `AuthController`

### POST /api/auth/check-approval-status

Check whether a student's account has been approved, denied, or is still pending. This endpoint does **not** require authentication and is intended to be called from login screens before or after credential verification.

**Authentication:** None

**Request Body:**

```json
{
  "email": "priya.sharma@example.com"
}
```

| Field   | Type     | Required | Description                |
|---------|----------|----------|----------------------------|
| `email` | `string` | Yes      | The student's email address|

**Response -- 200 OK:**

```json
{
  "success": true,
  "message": "Success",
  "data": {
    "approvalStatus": "Pending",
    "message": "Your account is pending admin approval. You will be notified once reviewed."
  },
  "errors": null
}
```

**Response -- Approved student:**

```json
{
  "success": true,
  "message": "Success",
  "data": {
    "approvalStatus": "Approved",
    "message": "Your account has been approved. You can now log in."
  },
  "errors": null
}
```

**Response -- Denied student:**

```json
{
  "success": true,
  "message": "Success",
  "data": {
    "approvalStatus": "Denied",
    "message": "Your account has been denied. Please contact support for more information."
  },
  "errors": null
}
```

**Response -- Email not found:**

```json
{
  "success": false,
  "message": "No account found with this email address.",
  "data": null,
  "errors": null
}
```

---

## 2. Student Endpoints

**Base path:** `/api/student`
**Controller:** `StudentController`
**Authentication:** All endpoints require **[Auth Required]** (JWT Bearer token)

---

### GET /api/student/profile

Retrieve the authenticated student's full profile.

**Authentication:** [Auth Required]

**Request:** No body. The user ID is extracted from the JWT token.

**Response -- 200 OK:**

```json
{
  "success": true,
  "message": "Success",
  "data": {
    "id": "a1b2c3d4-e5f6-7890-abcd-ef1234567890",
    "firstName": "Priya",
    "lastName": "Sharma",
    "email": "priya.sharma@example.com",
    "phoneNumber": "+91-9876543210",
    "role": "Student",
    "approvalStatus": "Approved",
    "emailVerified": true,
    "createdAt": "2026-01-15T10:30:00Z"
  },
  "errors": null
}
```

**Response Fields:**

| Field            | Type       | Description                            |
|------------------|------------|----------------------------------------|
| `id`             | `guid`     | Unique user identifier                 |
| `firstName`      | `string`   | Student's first name                   |
| `lastName`       | `string`   | Student's last name                    |
| `email`          | `string`   | Student's email address                |
| `phoneNumber`    | `string?`  | Phone number (nullable)                |
| `role`           | `string`   | User role (`Student` or `Admin`)       |
| `approvalStatus` | `string`   | `Pending`, `Approved`, or `Denied`     |
| `emailVerified`  | `boolean`  | Whether email has been OTP-verified    |
| `createdAt`      | `datetime` | Account creation timestamp (UTC)       |

---

### PUT /api/student/profile

Update the authenticated student's profile details.

**Authentication:** [Auth Required]

**Request Body:**

```json
{
  "firstName": "Priya",
  "lastName": "Sharma-Patel",
  "phoneNumber": "+91-9876543210"
}
```

| Field         | Type      | Required | Description                      |
|---------------|-----------|----------|----------------------------------|
| `firstName`   | `string`  | Yes      | Updated first name               |
| `lastName`    | `string`  | Yes      | Updated last name                |
| `phoneNumber` | `string?` | No       | Updated phone number (nullable)  |

**Response -- 200 OK:**

```json
{
  "success": true,
  "message": "Profile updated successfully",
  "data": {
    "id": "a1b2c3d4-e5f6-7890-abcd-ef1234567890",
    "firstName": "Priya",
    "lastName": "Sharma-Patel",
    "email": "priya.sharma@example.com",
    "phoneNumber": "+91-9876543210",
    "role": "Student",
    "approvalStatus": "Approved",
    "emailVerified": true,
    "createdAt": "2026-01-15T10:30:00Z"
  },
  "errors": null
}
```

**Response -- 400 Bad Request (validation):**

```json
{
  "success": false,
  "message": "Validation failed",
  "data": null,
  "errors": [
    "First name is required.",
    "Last name is required."
  ]
}
```

---

### GET /api/student/dashboard

Retrieve a summary dashboard for the authenticated student, including approval status and course registration count.

**Authentication:** [Auth Required]

**Request:** No body.

**Response -- 200 OK:**

```json
{
  "success": true,
  "message": "Success",
  "data": {
    "approvalStatus": "Approved",
    "registeredCoursesCount": 3,
    "firstName": "Priya",
    "lastName": "Sharma",
    "email": "priya.sharma@example.com"
  },
  "errors": null
}
```

**Response Fields:**

| Field                    | Type      | Description                              |
|--------------------------|-----------|------------------------------------------|
| `approvalStatus`         | `string`  | Current approval status                  |
| `registeredCoursesCount` | `integer` | Number of courses the student registered |
| `firstName`              | `string`  | Student's first name                     |
| `lastName`               | `string`  | Student's last name                      |
| `email`                  | `string`  | Student's email address                  |

---

### GET /api/student/courses

Retrieve the list of courses the authenticated student has registered for, including payment status for each.

**Authentication:** [Auth Required]

**Request:** No body.

**Response -- 200 OK:**

```json
{
  "success": true,
  "message": "Success",
  "data": [
    {
      "registrationId": "f1e2d3c4-b5a6-7890-fedc-ba0987654321",
      "courseId": "c1d2e3f4-a5b6-7890-abcd-1234567890ab",
      "courseName": "Full-Stack Web Development",
      "courseDescription": "Learn modern web development with React, Node.js, and databases.",
      "courseFees": 49999.00,
      "courseOfferPrice": 39999.00,
      "duration": "6 months",
      "paymentStatus": "Paid",
      "paymentAmount": 39999.00,
      "registeredAt": "2026-02-01T14:20:00Z"
    },
    {
      "registrationId": "a9b8c7d6-e5f4-3210-abcd-fedcba987654",
      "courseId": "d4e5f6a7-b8c9-0123-4567-89abcdef0123",
      "courseName": "Data Science with Python",
      "courseDescription": "Master data analysis, machine learning, and visualization with Python.",
      "courseFees": 59999.00,
      "courseOfferPrice": 44999.00,
      "duration": "8 months",
      "paymentStatus": "Pending",
      "paymentAmount": null,
      "registeredAt": "2026-03-10T09:15:00Z"
    }
  ],
  "errors": null
}
```

**Response Fields (per item):**

| Field               | Type       | Description                                         |
|---------------------|------------|-----------------------------------------------------|
| `registrationId`    | `guid`     | Unique course registration ID                       |
| `courseId`          | `guid`     | The course's unique identifier                      |
| `courseName`        | `string`   | Name of the course                                  |
| `courseDescription`  | `string?`  | Course description (nullable)                       |
| `courseFees`        | `decimal`  | Listed price of the course                          |
| `courseOfferPrice`   | `decimal?` | Discounted offer price (nullable)                   |
| `duration`          | `string?`  | Course duration (e.g., "6 months")                  |
| `paymentStatus`     | `string`   | `Pending`, `Paid`, `Partial`, or `Refunded`         |
| `paymentAmount`     | `decimal?` | Amount paid (nullable if unpaid)                    |
| `registeredAt`      | `datetime` | Registration timestamp (UTC)                        |

---

### POST /api/student/courses/register

Register the authenticated student for a course.

**Authentication:** [Auth Required]

**Request Body:**

```json
{
  "courseId": "c1d2e3f4-a5b6-7890-abcd-1234567890ab"
}
```

| Field      | Type   | Required | Description                         |
|------------|--------|----------|-------------------------------------|
| `courseId`  | `guid` | Yes      | ID of the course to register for    |

**Response -- 200 OK:**

```json
{
  "success": true,
  "message": "Successfully registered for the course",
  "data": {
    "registrationId": "f1e2d3c4-b5a6-7890-fedc-ba0987654321",
    "courseId": "c1d2e3f4-a5b6-7890-abcd-1234567890ab",
    "courseName": "Full-Stack Web Development",
    "courseDescription": "Learn modern web development with React, Node.js, and databases.",
    "courseFees": 49999.00,
    "courseOfferPrice": 39999.00,
    "duration": "6 months",
    "paymentStatus": "Pending",
    "paymentAmount": null,
    "registeredAt": "2026-03-14T11:00:00Z"
  },
  "errors": null
}
```

**Response -- 400 Bad Request (already registered):**

```json
{
  "success": false,
  "message": "You are already registered for this course.",
  "data": null,
  "errors": null
}
```

**Response -- 400 Bad Request (course not found):**

```json
{
  "success": false,
  "message": "Course not found or is no longer active.",
  "data": null,
  "errors": null
}
```

---

## 3. Admin Endpoints

**Base path:** `/api/admin`
**Controller:** `AdminController`
**Authentication:** All endpoints require **[Auth Required]** + **[Admin Role]**

Non-admin users will receive a `403 Forbidden` response.

---

### GET /api/admin/dashboard

Retrieve aggregate statistics for the admin dashboard.

**Authentication:** [Auth Required] [Admin]

**Request:** No body.

**Response -- 200 OK:**

```json
{
  "success": true,
  "message": "Success",
  "data": {
    "totalStudents": 156,
    "pendingApprovals": 12,
    "approvedStudents": 138,
    "deniedStudents": 6,
    "totalCourses": 8,
    "totalRegistrations": 243
  },
  "errors": null
}
```

**Response Fields:**

| Field                | Type      | Description                                |
|----------------------|-----------|--------------------------------------------|
| `totalStudents`      | `integer` | Total registered students                  |
| `pendingApprovals`   | `integer` | Students awaiting admin review             |
| `approvedStudents`   | `integer` | Students who have been approved            |
| `deniedStudents`     | `integer` | Students who have been denied              |
| `totalCourses`       | `integer` | Total courses in the system                |
| `totalRegistrations` | `integer` | Total course registrations across all students |

---

### GET /api/admin/students

Retrieve a paginated, filterable list of students.

**Authentication:** [Auth Required] [Admin]

**Query Parameters:**

| Parameter  | Type      | Default | Description                                               |
|------------|-----------|---------|-----------------------------------------------------------|
| `page`     | `integer` | `1`     | Page number (1-based)                                     |
| `pageSize` | `integer` | `10`    | Number of results per page                                |
| `status`   | `string?` | `null`  | Filter by approval status: `Pending`, `Approved`, `Denied`|
| `search`   | `string?` | `null`  | Search by name or email (partial match)                   |

**Example Request:**

```
GET /api/admin/students?page=1&pageSize=10&status=Pending&search=priya
```

**Response -- 200 OK:**

```json
{
  "success": true,
  "message": "Success",
  "data": {
    "items": [
      {
        "id": "a1b2c3d4-e5f6-7890-abcd-ef1234567890",
        "firstName": "Priya",
        "lastName": "Sharma",
        "email": "priya.sharma@example.com",
        "phoneNumber": "+91-9876543210",
        "approvalStatus": "Pending",
        "emailVerified": true,
        "isActive": true,
        "createdAt": "2026-03-12T08:45:00Z"
      },
      {
        "id": "b2c3d4e5-f6a7-8901-bcde-f12345678901",
        "firstName": "Priyanka",
        "lastName": "Reddy",
        "email": "priyanka.reddy@example.com",
        "phoneNumber": null,
        "approvalStatus": "Pending",
        "emailVerified": true,
        "isActive": true,
        "createdAt": "2026-03-13T16:20:00Z"
      }
    ],
    "totalCount": 2,
    "page": 1,
    "pageSize": 10,
    "totalPages": 1,
    "hasNextPage": false,
    "hasPreviousPage": false
  },
  "errors": null
}
```

**Response Fields (per item in `items`):**

| Field            | Type       | Description                          |
|------------------|------------|--------------------------------------|
| `id`             | `guid`     | Student's unique identifier          |
| `firstName`      | `string`   | First name                           |
| `lastName`       | `string`   | Last name                            |
| `email`          | `string`   | Email address                        |
| `phoneNumber`    | `string?`  | Phone number (nullable)              |
| `approvalStatus` | `string`   | `Pending`, `Approved`, or `Denied`   |
| `emailVerified`  | `boolean`  | Whether email has been verified      |
| `isActive`       | `boolean`  | Whether the account is active        |
| `createdAt`      | `datetime` | Account creation timestamp (UTC)     |

---

### GET /api/admin/students/{id}

Retrieve full details for a specific student, including their registered courses.

**Authentication:** [Auth Required] [Admin]

**Path Parameters:**

| Parameter | Type   | Description                     |
|-----------|--------|---------------------------------|
| `id`      | `guid` | The student's unique identifier |

**Example Request:**

```
GET /api/admin/students/a1b2c3d4-e5f6-7890-abcd-ef1234567890
```

**Response -- 200 OK:**

```json
{
  "success": true,
  "message": "Success",
  "data": {
    "id": "a1b2c3d4-e5f6-7890-abcd-ef1234567890",
    "firstName": "Priya",
    "lastName": "Sharma",
    "email": "priya.sharma@example.com",
    "phoneNumber": "+91-9876543210",
    "approvalStatus": "Approved",
    "denialReason": null,
    "emailVerified": true,
    "isActive": true,
    "createdAt": "2026-01-15T10:30:00Z",
    "lastLoginAt": "2026-03-14T07:12:00Z",
    "registeredCourses": [
      {
        "registrationId": "f1e2d3c4-b5a6-7890-fedc-ba0987654321",
        "courseId": "c1d2e3f4-a5b6-7890-abcd-1234567890ab",
        "courseName": "Full-Stack Web Development",
        "courseDescription": "Learn modern web development with React, Node.js, and databases.",
        "courseFees": 49999.00,
        "courseOfferPrice": 39999.00,
        "duration": "6 months",
        "paymentStatus": "Paid",
        "paymentAmount": 39999.00,
        "registeredAt": "2026-02-01T14:20:00Z"
      },
      {
        "registrationId": "a9b8c7d6-e5f4-3210-abcd-fedcba987654",
        "courseId": "d4e5f6a7-b8c9-0123-4567-89abcdef0123",
        "courseName": "Data Science with Python",
        "courseDescription": "Master data analysis, machine learning, and visualization with Python.",
        "courseFees": 59999.00,
        "courseOfferPrice": 44999.00,
        "duration": "8 months",
        "paymentStatus": "Pending",
        "paymentAmount": null,
        "registeredAt": "2026-03-10T09:15:00Z"
      }
    ]
  },
  "errors": null
}
```

**Response Fields:**

| Field              | Type                      | Description                                  |
|--------------------|---------------------------|----------------------------------------------|
| `id`               | `guid`                    | Student's unique identifier                  |
| `firstName`        | `string`                  | First name                                   |
| `lastName`         | `string`                  | Last name                                    |
| `email`            | `string`                  | Email address                                |
| `phoneNumber`      | `string?`                 | Phone number (nullable)                      |
| `approvalStatus`   | `string`                  | `Pending`, `Approved`, or `Denied`           |
| `denialReason`     | `string?`                 | Reason provided when denied (nullable)       |
| `emailVerified`    | `boolean`                 | Whether email has been verified              |
| `isActive`         | `boolean`                 | Whether the account is active                |
| `createdAt`        | `datetime`                | Account creation timestamp (UTC)             |
| `lastLoginAt`      | `datetime?`               | Last login timestamp (nullable if never)     |
| `registeredCourses`| `StudentCourseResponse[]` | Array of course registrations (see Student Courses) |

**Response -- 400 Bad Request (not found):**

```json
{
  "success": false,
  "message": "Student not found.",
  "data": null,
  "errors": null
}
```

---

### PUT /api/admin/students/{id}

Toggle a student's active status (enable or disable the account).

**Authentication:** [Auth Required] [Admin]

**Path Parameters:**

| Parameter | Type   | Description                     |
|-----------|--------|---------------------------------|
| `id`      | `guid` | The student's unique identifier |

**Request Body:**

```json
{
  "isActive": false
}
```

| Field      | Type      | Required | Description                                 |
|------------|-----------|----------|---------------------------------------------|
| `isActive` | `boolean` | Yes      | `true` to activate, `false` to deactivate   |

**Response -- 200 OK:**

```json
{
  "success": true,
  "message": "Student updated successfully",
  "data": null,
  "errors": null
}
```

---

### POST /api/admin/students/{id}/approve

Approve a pending student. This changes their `approvalStatus` from `Pending` to `Approved` and triggers a notification.

**Authentication:** [Auth Required] [Admin]

**Path Parameters:**

| Parameter | Type   | Description                     |
|-----------|--------|---------------------------------|
| `id`      | `guid` | The student's unique identifier |

**Request:** No body required.

**Response -- 200 OK:**

```json
{
  "success": true,
  "message": "Student approved successfully",
  "data": null,
  "errors": null
}
```

**Response -- 400 Bad Request (already approved):**

```json
{
  "success": false,
  "message": "Student is not in Pending status.",
  "data": null,
  "errors": null
}
```

---

### POST /api/admin/students/{id}/deny

Deny a pending student with an optional reason. This changes their `approvalStatus` from `Pending` to `Denied`.

**Authentication:** [Auth Required] [Admin]

**Path Parameters:**

| Parameter | Type   | Description                     |
|-----------|--------|---------------------------------|
| `id`      | `guid` | The student's unique identifier |

**Request Body:**

```json
{
  "reason": "Incomplete documentation. Please re-apply with valid ID proof."
}
```

| Field    | Type      | Required | Description                                |
|----------|-----------|----------|--------------------------------------------|
| `reason` | `string?` | No       | Optional explanation for the denial        |

**Response -- 200 OK:**

```json
{
  "success": true,
  "message": "Student denied successfully",
  "data": null,
  "errors": null
}
```

**Response -- 400 Bad Request (not pending):**

```json
{
  "success": false,
  "message": "Student is not in Pending status.",
  "data": null,
  "errors": null
}
```

---

### GET /api/admin/notifications

Retrieve all notifications for the authenticated admin, ordered by most recent first.

**Authentication:** [Auth Required] [Admin]

**Request:** No body.

**Response -- 200 OK:**

```json
{
  "success": true,
  "message": "Success",
  "data": [
    {
      "id": "n1a2b3c4-d5e6-7890-abcd-ef1234567890",
      "type": "NewRegistration",
      "title": "New Student Registration",
      "message": "Priya Sharma (priya.sharma@example.com) has registered and is awaiting approval.",
      "referenceId": "a1b2c3d4-e5f6-7890-abcd-ef1234567890",
      "isRead": false,
      "createdAt": "2026-03-14T08:30:00Z"
    },
    {
      "id": "n2b3c4d5-e6f7-8901-bcde-f12345678901",
      "type": "CourseRegistration",
      "title": "New Course Registration",
      "message": "Rahul Verma registered for Full-Stack Web Development.",
      "referenceId": "f1e2d3c4-b5a6-7890-fedc-ba0987654321",
      "isRead": true,
      "createdAt": "2026-03-13T15:45:00Z"
    }
  ],
  "errors": null
}
```

**Response Fields (per item):**

| Field         | Type       | Description                                         |
|---------------|------------|-----------------------------------------------------|
| `id`          | `guid`     | Notification unique identifier                      |
| `type`        | `string`   | Notification type (see [NotificationType](#notificationtype)) |
| `title`       | `string`   | Short notification title                            |
| `message`     | `string?`  | Detailed notification message (nullable)            |
| `referenceId` | `guid?`    | ID of the related entity (user or registration)     |
| `isRead`      | `boolean`  | Whether the admin has read this notification        |
| `createdAt`   | `datetime` | Notification creation timestamp (UTC)               |

---

### GET /api/admin/notifications/unread-count

Get the count of unread notifications for the authenticated admin. Useful for notification badge display.

**Authentication:** [Auth Required] [Admin]

**Request:** No body.

**Response -- 200 OK:**

```json
{
  "success": true,
  "message": "Success",
  "data": {
    "count": 5
  },
  "errors": null
}
```

| Field   | Type      | Description                   |
|---------|-----------|-------------------------------|
| `count` | `integer` | Number of unread notifications|

---

### PUT /api/admin/notifications/{id}/read

Mark a specific notification as read.

**Authentication:** [Auth Required] [Admin]

**Path Parameters:**

| Parameter | Type   | Description                          |
|-----------|--------|--------------------------------------|
| `id`      | `guid` | The notification's unique identifier |

**Request:** No body.

**Response -- 200 OK:**

```json
{
  "success": true,
  "message": "Notification marked as read",
  "data": null,
  "errors": null
}
```

**Response -- 400 Bad Request (not found):**

```json
{
  "success": false,
  "message": "Notification not found.",
  "data": null,
  "errors": null
}
```

---

### POST /api/admin/courses

Create a new course. The `syllabus` field accepts a JSON string representing structured syllabus data.

**Authentication:** [Auth Required] [Admin]

**Request Body:**

```json
{
  "name": "Full-Stack Web Development",
  "description": "Learn modern web development with React, Node.js, and databases. Build production-ready applications from scratch.",
  "fees": 49999.00,
  "offerPrice": 39999.00,
  "syllabus": "[{\"module\":\"HTML & CSS Fundamentals\",\"topics\":[\"Semantic HTML5\",\"CSS Grid & Flexbox\",\"Responsive Design\"]},{\"module\":\"JavaScript & TypeScript\",\"topics\":[\"ES6+ Features\",\"TypeScript Basics\",\"Async Programming\"]},{\"module\":\"React.js\",\"topics\":[\"Components & Hooks\",\"State Management\",\"React Router\"]},{\"module\":\"Node.js & Express\",\"topics\":[\"REST APIs\",\"Authentication\",\"Database Integration\"]}]",
  "duration": "6 months"
}
```

| Field         | Type       | Required | Description                                              |
|---------------|------------|----------|----------------------------------------------------------|
| `name`        | `string`   | Yes      | Course name                                              |
| `description` | `string?`  | No       | Course description                                       |
| `fees`        | `decimal`  | Yes      | Listed course fee                                        |
| `offerPrice`  | `decimal?` | No       | Discounted price (nullable, omit if no discount)         |
| `syllabus`    | `string?`  | No       | JSON string containing structured syllabus data          |
| `duration`    | `string?`  | No       | Course duration (e.g., "6 months", "12 weeks")           |

**Response -- 200 OK:**

```json
{
  "success": true,
  "message": "Course created successfully",
  "data": {
    "id": "c1d2e3f4-a5b6-7890-abcd-1234567890ab",
    "name": "Full-Stack Web Development",
    "description": "Learn modern web development with React, Node.js, and databases. Build production-ready applications from scratch.",
    "fees": 49999.00,
    "offerPrice": 39999.00,
    "syllabus": "[{\"module\":\"HTML & CSS Fundamentals\",\"topics\":[\"Semantic HTML5\",\"CSS Grid & Flexbox\",\"Responsive Design\"]},{\"module\":\"JavaScript & TypeScript\",\"topics\":[\"ES6+ Features\",\"TypeScript Basics\",\"Async Programming\"]},{\"module\":\"React.js\",\"topics\":[\"Components & Hooks\",\"State Management\",\"React Router\"]},{\"module\":\"Node.js & Express\",\"topics\":[\"REST APIs\",\"Authentication\",\"Database Integration\"]}]",
    "duration": "6 months",
    "isActive": true,
    "createdAt": "2026-03-14T10:00:00Z",
    "updatedAt": null
  },
  "errors": null
}
```

---

### PUT /api/admin/courses/{id}

Update an existing course.

**Authentication:** [Auth Required] [Admin]

**Path Parameters:**

| Parameter | Type   | Description                     |
|-----------|--------|---------------------------------|
| `id`      | `guid` | The course's unique identifier  |

**Request Body:**

```json
{
  "name": "Full-Stack Web Development (Updated)",
  "description": "Comprehensive full-stack course covering React, Node.js, databases, and deployment.",
  "fees": 54999.00,
  "offerPrice": 42999.00,
  "syllabus": "[{\"module\":\"HTML & CSS Fundamentals\",\"topics\":[\"Semantic HTML5\",\"CSS Grid & Flexbox\",\"Responsive Design\"]},{\"module\":\"JavaScript & TypeScript\",\"topics\":[\"ES6+ Features\",\"TypeScript Basics\",\"Async Programming\"]},{\"module\":\"React.js\",\"topics\":[\"Components & Hooks\",\"State Management\",\"React Router\",\"Next.js Intro\"]},{\"module\":\"Node.js & Express\",\"topics\":[\"REST APIs\",\"Authentication\",\"Database Integration\"]},{\"module\":\"DevOps & Deployment\",\"topics\":[\"Docker Basics\",\"CI/CD\",\"Cloud Deployment\"]}]",
  "duration": "7 months",
  "isActive": true
}
```

| Field         | Type       | Required | Description                                       |
|---------------|------------|----------|---------------------------------------------------|
| `name`        | `string`   | Yes      | Updated course name                               |
| `description` | `string?`  | No       | Updated description                               |
| `fees`        | `decimal`  | Yes      | Updated course fee                                |
| `offerPrice`  | `decimal?` | No       | Updated offer price (nullable)                    |
| `syllabus`    | `string?`  | No       | Updated syllabus JSON string                      |
| `duration`    | `string?`  | No       | Updated duration                                  |
| `isActive`    | `boolean`  | Yes      | Whether the course is active (default `true`)     |

**Response -- 200 OK:**

```json
{
  "success": true,
  "message": "Course updated successfully",
  "data": {
    "id": "c1d2e3f4-a5b6-7890-abcd-1234567890ab",
    "name": "Full-Stack Web Development (Updated)",
    "description": "Comprehensive full-stack course covering React, Node.js, databases, and deployment.",
    "fees": 54999.00,
    "offerPrice": 42999.00,
    "syllabus": "[{\"module\":\"HTML & CSS Fundamentals\",\"topics\":[\"Semantic HTML5\",\"CSS Grid & Flexbox\",\"Responsive Design\"]},{\"module\":\"JavaScript & TypeScript\",\"topics\":[\"ES6+ Features\",\"TypeScript Basics\",\"Async Programming\"]},{\"module\":\"React.js\",\"topics\":[\"Components & Hooks\",\"State Management\",\"React Router\",\"Next.js Intro\"]},{\"module\":\"Node.js & Express\",\"topics\":[\"REST APIs\",\"Authentication\",\"Database Integration\"]},{\"module\":\"DevOps & Deployment\",\"topics\":[\"Docker Basics\",\"CI/CD\",\"Cloud Deployment\"]}]",
    "duration": "7 months",
    "isActive": true,
    "createdAt": "2026-03-14T10:00:00Z",
    "updatedAt": "2026-03-14T12:30:00Z"
  },
  "errors": null
}
```

**Response -- 400 Bad Request (not found):**

```json
{
  "success": false,
  "message": "Course not found.",
  "data": null,
  "errors": null
}
```

---

### DELETE /api/admin/courses/{id}

Soft-delete a course. The course is marked as inactive and deleted but remains in the database. It will no longer appear in active course listings.

**Authentication:** [Auth Required] [Admin]

**Path Parameters:**

| Parameter | Type   | Description                     |
|-----------|--------|---------------------------------|
| `id`      | `guid` | The course's unique identifier  |

**Request:** No body.

**Response -- 200 OK:**

```json
{
  "success": true,
  "message": "Course deleted successfully",
  "data": null,
  "errors": null
}
```

**Response -- 400 Bad Request (not found):**

```json
{
  "success": false,
  "message": "Course not found.",
  "data": null,
  "errors": null
}
```

---

### GET /api/admin/course-registrations

Retrieve a paginated list of all course registrations across all students.

**Authentication:** [Auth Required] [Admin]

**Query Parameters:**

| Parameter  | Type      | Default | Description                |
|------------|-----------|---------|----------------------------|
| `page`     | `integer` | `1`     | Page number (1-based)      |
| `pageSize` | `integer` | `10`    | Number of results per page |

**Example Request:**

```
GET /api/admin/course-registrations?page=1&pageSize=10
```

**Response -- 200 OK:**

```json
{
  "success": true,
  "message": "Success",
  "data": {
    "items": [
      {
        "id": "f1e2d3c4-b5a6-7890-fedc-ba0987654321",
        "userId": "a1b2c3d4-e5f6-7890-abcd-ef1234567890",
        "studentName": "Priya Sharma",
        "studentEmail": "priya.sharma@example.com",
        "courseId": "c1d2e3f4-a5b6-7890-abcd-1234567890ab",
        "courseName": "Full-Stack Web Development",
        "paymentStatus": "Paid",
        "paymentAmount": 39999.00,
        "paymentDate": "2026-02-05T11:00:00Z",
        "notes": "Paid via bank transfer. Ref: TXN20260205001",
        "registeredAt": "2026-02-01T14:20:00Z"
      },
      {
        "id": "a9b8c7d6-e5f4-3210-abcd-fedcba987654",
        "userId": "e5f6a7b8-c9d0-1234-5678-9abcdef01234",
        "studentName": "Rahul Verma",
        "studentEmail": "rahul.verma@example.com",
        "courseId": "d4e5f6a7-b8c9-0123-4567-89abcdef0123",
        "courseName": "Data Science with Python",
        "paymentStatus": "Partial",
        "paymentAmount": 20000.00,
        "paymentDate": "2026-03-01T09:30:00Z",
        "notes": "First installment received. Balance: 24999.00",
        "registeredAt": "2026-02-28T16:10:00Z"
      },
      {
        "id": "b3c4d5e6-f7a8-9012-cdef-012345678901",
        "userId": "f6a7b8c9-d0e1-2345-6789-abcdef012345",
        "studentName": "Ananya Iyer",
        "studentEmail": "ananya.iyer@example.com",
        "courseId": "c1d2e3f4-a5b6-7890-abcd-1234567890ab",
        "courseName": "Full-Stack Web Development",
        "paymentStatus": "Pending",
        "paymentAmount": null,
        "paymentDate": null,
        "notes": null,
        "registeredAt": "2026-03-13T10:45:00Z"
      }
    ],
    "totalCount": 243,
    "page": 1,
    "pageSize": 10,
    "totalPages": 25,
    "hasNextPage": true,
    "hasPreviousPage": false
  },
  "errors": null
}
```

**Response Fields (per item in `items`):**

| Field           | Type        | Description                                      |
|-----------------|-------------|--------------------------------------------------|
| `id`            | `guid`      | Course registration unique identifier            |
| `userId`        | `guid`      | The student's user ID                            |
| `studentName`   | `string`    | Full name of the student                         |
| `studentEmail`  | `string`    | Email of the student                             |
| `courseId`       | `guid`      | The course ID                                    |
| `courseName`    | `string`    | Name of the course                               |
| `paymentStatus` | `string`    | `Pending`, `Paid`, `Partial`, or `Refunded`      |
| `paymentAmount` | `decimal?`  | Amount paid (nullable if unpaid)                 |
| `paymentDate`   | `datetime?` | When the payment was recorded (nullable)         |
| `notes`         | `string?`   | Admin notes about the payment (nullable)         |
| `registeredAt`  | `datetime`  | When the student registered for the course (UTC) |

---

### PUT /api/admin/course-registrations/{id}/payment

Update the payment status and details for a course registration.

**Authentication:** [Auth Required] [Admin]

**Path Parameters:**

| Parameter | Type   | Description                               |
|-----------|--------|-------------------------------------------|
| `id`      | `guid` | The course registration's unique identifier|

**Request Body:**

```json
{
  "paymentStatus": "Paid",
  "paymentAmount": 39999.00,
  "notes": "Full payment received via UPI. Ref: UPI20260314001"
}
```

| Field           | Type       | Required | Description                                      |
|-----------------|------------|----------|--------------------------------------------------|
| `paymentStatus` | `string`   | Yes      | New status: `Pending`, `Paid`, `Partial`, or `Refunded` |
| `paymentAmount` | `decimal?` | No       | Payment amount (nullable)                        |
| `notes`         | `string?`  | No       | Admin notes about the payment                    |

**Response -- 200 OK:**

```json
{
  "success": true,
  "message": "Payment updated successfully",
  "data": null,
  "errors": null
}
```

**Response -- 400 Bad Request (invalid status):**

```json
{
  "success": false,
  "message": "Invalid payment status. Allowed values: Pending, Paid, Partial, Refunded.",
  "data": null,
  "errors": null
}
```

**Response -- 400 Bad Request (not found):**

```json
{
  "success": false,
  "message": "Course registration not found.",
  "data": null,
  "errors": null
}
```

---

## 4. Course Endpoints

**Base path:** `/api/course`
**Controller:** `CourseController`
**Authentication:** All endpoints require **[Auth Required]** (JWT Bearer token)

> **Note:** The route is `/api/course` (singular) as derived from the `CourseController` class name via the `[controller]` route token.

---

### GET /api/course

Retrieve the list of all active courses. Soft-deleted and inactive courses are excluded.

**Authentication:** [Auth Required]

**Request:** No body.

**Response -- 200 OK:**

```json
{
  "success": true,
  "message": "Success",
  "data": [
    {
      "id": "c1d2e3f4-a5b6-7890-abcd-1234567890ab",
      "name": "Full-Stack Web Development",
      "description": "Learn modern web development with React, Node.js, and databases.",
      "fees": 49999.00,
      "offerPrice": 39999.00,
      "duration": "6 months",
      "isActive": true
    },
    {
      "id": "d4e5f6a7-b8c9-0123-4567-89abcdef0123",
      "name": "Data Science with Python",
      "description": "Master data analysis, machine learning, and visualization with Python.",
      "fees": 59999.00,
      "offerPrice": 44999.00,
      "duration": "8 months",
      "isActive": true
    },
    {
      "id": "e5f6a7b8-c9d0-1234-5678-abcdef012345",
      "name": "Mobile App Development with .NET MAUI",
      "description": "Build cross-platform mobile applications using .NET MAUI and C#.",
      "fees": 44999.00,
      "offerPrice": null,
      "duration": "5 months",
      "isActive": true
    }
  ],
  "errors": null
}
```

**Response Fields (per item):**

| Field         | Type       | Description                                   |
|---------------|------------|-----------------------------------------------|
| `id`          | `guid`     | Course unique identifier                      |
| `name`        | `string`   | Course name                                   |
| `description` | `string?`  | Course description (nullable)                 |
| `fees`        | `decimal`  | Listed course fee                             |
| `offerPrice`  | `decimal?` | Discounted offer price (nullable if none)     |
| `duration`    | `string?`  | Course duration (nullable)                    |
| `isActive`    | `boolean`  | Always `true` for this endpoint               |

---

### GET /api/course/{id}

Retrieve full details of a specific course, including the syllabus.

**Authentication:** [Auth Required]

**Path Parameters:**

| Parameter | Type   | Description                    |
|-----------|--------|--------------------------------|
| `id`      | `guid` | The course's unique identifier |

**Example Request:**

```
GET /api/course/c1d2e3f4-a5b6-7890-abcd-1234567890ab
```

**Response -- 200 OK:**

```json
{
  "success": true,
  "message": "Success",
  "data": {
    "id": "c1d2e3f4-a5b6-7890-abcd-1234567890ab",
    "name": "Full-Stack Web Development",
    "description": "Learn modern web development with React, Node.js, and databases. Build production-ready applications from scratch.",
    "fees": 49999.00,
    "offerPrice": 39999.00,
    "syllabus": "[{\"module\":\"HTML & CSS Fundamentals\",\"topics\":[\"Semantic HTML5\",\"CSS Grid & Flexbox\",\"Responsive Design\"]},{\"module\":\"JavaScript & TypeScript\",\"topics\":[\"ES6+ Features\",\"TypeScript Basics\",\"Async Programming\"]},{\"module\":\"React.js\",\"topics\":[\"Components & Hooks\",\"State Management\",\"React Router\"]},{\"module\":\"Node.js & Express\",\"topics\":[\"REST APIs\",\"Authentication\",\"Database Integration\"]}]",
    "duration": "6 months",
    "isActive": true,
    "createdAt": "2026-03-14T10:00:00Z",
    "updatedAt": null
  },
  "errors": null
}
```

**Syllabus JSON Structure:**

The `syllabus` field is a JSON string. When parsed, it contains an array of module objects:

```json
[
  {
    "module": "HTML & CSS Fundamentals",
    "topics": [
      "Semantic HTML5",
      "CSS Grid & Flexbox",
      "Responsive Design"
    ]
  },
  {
    "module": "JavaScript & TypeScript",
    "topics": [
      "ES6+ Features",
      "TypeScript Basics",
      "Async Programming"
    ]
  }
]
```

**Response Fields:**

| Field         | Type        | Description                                     |
|---------------|-------------|-------------------------------------------------|
| `id`          | `guid`      | Course unique identifier                        |
| `name`        | `string`    | Course name                                     |
| `description` | `string?`   | Course description (nullable)                   |
| `fees`        | `decimal`   | Listed course fee                               |
| `offerPrice`  | `decimal?`  | Discounted offer price (nullable if none)       |
| `syllabus`    | `string?`   | JSON string containing module/topic structure   |
| `duration`    | `string?`   | Course duration (nullable)                      |
| `isActive`    | `boolean`   | Whether the course is active                    |
| `createdAt`   | `datetime`  | Course creation timestamp (UTC)                 |
| `updatedAt`   | `datetime?` | Last update timestamp (nullable if never updated)|

**Response -- 400 Bad Request (not found):**

```json
{
  "success": false,
  "message": "Course not found.",
  "data": null,
  "errors": null
}
```

---

## Error Handling

### HTTP Status Codes

| Status Code | Meaning                  | When Returned                                        |
|-------------|--------------------------|------------------------------------------------------|
| `200`       | OK                       | Successful operation                                 |
| `400`       | Bad Request              | Validation error, business rule violation, not found |
| `401`       | Unauthorized             | Missing or expired JWT token                         |
| `403`       | Forbidden                | Authenticated user lacks required role               |
| `500`       | Internal Server Error    | Unhandled server exception                           |

### Global Exception Handling

The API uses the `ExceptionHandlingMiddleware` to catch unhandled exceptions and return a consistent error response:

```json
{
  "success": false,
  "message": "An unexpected error occurred. Please try again later.",
  "data": null,
  "errors": null
}
```

### Validation Errors

Validation is handled by FluentValidation. When request body validation fails, the API returns `400 Bad Request` with the specific field errors in the `errors` array:

```json
{
  "success": false,
  "message": "Validation failed",
  "data": null,
  "errors": [
    "'First Name' must not be empty.",
    "'Last Name' must not be empty.",
    "'Email' is not a valid email address."
  ]
}
```

### Authentication Errors

**Missing token:**

```
HTTP/1.1 401 Unauthorized
WWW-Authenticate: Bearer
```

**Expired token:**

```
HTTP/1.1 401 Unauthorized
WWW-Authenticate: Bearer error="invalid_token", error_description="The token has expired"
```

**Insufficient role (e.g., Student calling Admin endpoint):**

```
HTTP/1.1 403 Forbidden
```

---

## Quick Reference: All Phase 2 Endpoints

| Method   | Endpoint                                      | Auth        | Description                          |
|----------|-----------------------------------------------|-------------|--------------------------------------|
| `POST`   | `/api/auth/check-approval-status`             | None        | Check student approval status        |
| `GET`    | `/api/student/profile`                        | Bearer      | Get student profile                  |
| `PUT`    | `/api/student/profile`                        | Bearer      | Update student profile               |
| `GET`    | `/api/student/dashboard`                      | Bearer      | Get student dashboard summary        |
| `GET`    | `/api/student/courses`                        | Bearer      | List student's registered courses    |
| `POST`   | `/api/student/courses/register`               | Bearer      | Register for a course                |
| `GET`    | `/api/admin/dashboard`                        | Bearer+Admin| Get admin dashboard stats            |
| `GET`    | `/api/admin/students`                         | Bearer+Admin| List students (paginated, filtered)  |
| `GET`    | `/api/admin/students/{id}`                    | Bearer+Admin| Get student detail with courses      |
| `PUT`    | `/api/admin/students/{id}`                    | Bearer+Admin| Toggle student active status         |
| `POST`   | `/api/admin/students/{id}/approve`            | Bearer+Admin| Approve a pending student            |
| `POST`   | `/api/admin/students/{id}/deny`               | Bearer+Admin| Deny a pending student               |
| `GET`    | `/api/admin/notifications`                    | Bearer+Admin| List admin notifications             |
| `GET`    | `/api/admin/notifications/unread-count`       | Bearer+Admin| Get unread notification count        |
| `PUT`    | `/api/admin/notifications/{id}/read`          | Bearer+Admin| Mark notification as read            |
| `POST`   | `/api/admin/courses`                          | Bearer+Admin| Create a new course                  |
| `PUT`    | `/api/admin/courses/{id}`                     | Bearer+Admin| Update an existing course            |
| `DELETE` | `/api/admin/courses/{id}`                     | Bearer+Admin| Soft-delete a course                 |
| `GET`    | `/api/admin/course-registrations`             | Bearer+Admin| List all course registrations        |
| `PUT`    | `/api/admin/course-registrations/{id}/payment`| Bearer+Admin| Update payment for a registration    |
| `GET`    | `/api/course`                                 | Bearer      | List all active courses              |
| `GET`    | `/api/course/{id}`                            | Bearer      | Get course detail with syllabus      |

---

## Integration Notes for Frontend / Mobile Developers

1. **Token management:** Access tokens expire after **15 minutes**. Refresh tokens are valid for **7 days**. Use the Phase 1 `POST /api/auth/refresh-token` endpoint to obtain a new access token before it expires.

2. **CORS:** The API is configured with an `AllowAll` CORS policy during development, permitting any origin, method, and header. This will be restricted in production.

3. **Syllabus parsing:** The `syllabus` field in course responses is a JSON **string**, not a parsed object. You must call `JSON.parse(syllabus)` (or your platform's equivalent) to work with the structured data.

4. **GUIDs:** All identifiers (`id`, `courseId`, `userId`, etc.) are UUIDs in the format `xxxxxxxx-xxxx-xxxx-xxxx-xxxxxxxxxxxx`.

5. **Nullable fields:** Fields marked as nullable (`string?`, `decimal?`, `datetime?`) may be `null` in the JSON response. Always handle null values gracefully in your UI.

6. **Pagination defaults:** If `page` and `pageSize` are omitted from paginated requests, they default to `1` and `10` respectively.

7. **Approval flow:** New students start with `ApprovalStatus = "Pending"`. They can authenticate but may have restricted access until an admin approves them. Use `POST /api/auth/check-approval-status` to check before allowing full app access.

8. **Swagger UI:** In development, interactive API documentation is available at `/swagger`.
