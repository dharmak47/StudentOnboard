# Course Completion Certificate POC

This document outlines the implementation plan for building a Proof of Concept (POC) for the Certificate Download feature identified during UAT. 

## Goal
To implement a backend API endpoint that dynamically generates a high-quality PDF Certificate of Completion when a student successfully finishes a course. 

> [!NOTE]
> This POC will be integrated directly into your existing `.NET 8` backend so you can test it with real student and course data.

## Proposed Changes

### 1. New Dependency
We will add **QuestPDF**, an open-source, modern .NET library for generating beautiful PDF documents.

### 2. Services Layer
#### [NEW] `ICertificateService.cs` (Interfaces)
- Defines the method to generate the certificate: `Task<ApiResponse<byte[]>> GenerateCertificateAsync(Guid registrationId, Guid studentId)`

#### [NEW] `CertificateService.cs` (Implementations)
- Validates that the `CourseRegistration` belongs to the student and has `IsCompleted = true`.
- Fetches the Student's name and the Course details from the respective repositories.
- Uses QuestPDF's fluent API to design a professional certificate layout (borders, headers, signatures, and dates).
- Returns the generated PDF as a byte array.

### 3. API Controller Layer
#### [NEW] `CertificatesController.cs` (Controllers)
- Exposes a new authenticated endpoint: `GET /api/certificates/download/{registrationId}`
- Returns a `FileStreamResult` with the MIME type `application/pdf`, allowing the browser to download it as `Certificate_{CourseName}.pdf`.

### 4. Dependency Injection
#### [MODIFY] `Program.cs`
- Register `ICertificateService` and `CertificateService`.
- Configure QuestPDF to use the community license.

---

## User Review Required

> [!IMPORTANT]
> **QuestPDF License:** QuestPDF is completely free to use for personal projects, POCs, and companies with less than $1M USD annual gross revenue (Community License). Does this fit your current company profile? 

## Open Questions

> [!TIP]
> Since this is a POC, I will create a clean, minimalist design for the certificate. If you have specific brand colors or a company logo you want on the certificate later, you will be able to easily plug those in!

## Verification Plan

### Automated Tests
* We will verify the API compiles and runs correctly.

### Manual Verification
* We will use a script to mark `vmalini581@gmail.com`'s course registration as `IsCompleted = true` and `CompletedAt = GETDATE()`.
* We will generate a real PDF certificate file using the API and save it to your desktop so you can open and review the final output!
