# Student Onboarding Platform - Final User Manual

This comprehensive guide covers the entire end-to-end functionality of the Student Onboarding Platform, incorporating all features developed for both Students (Mobile App) and Administrators (Mobile App & Web Portal).

## 1. Student Registration & Onboarding

**Signing Up & OTP Verification (Student)**
* The student downloads and opens the Mobile App and taps **Sign Up**.
* They enter their personal details (Name, Email, Phone, Password) and select a preferred course.
* The system sends a 6-digit OTP to their email. The student enters this code to verify their identity. *(If delayed, they can request a resend after 60 seconds).*
* Upon successful verification, the account enters a **Pending** status.

**Administrator Approval (Admin)**
* The administrator is notified of a new pending registration.
* The administrator verifies the student's details (and any offline prerequisites like initial fees) and approves the account in the system.
* The student's app automatically updates, guiding them through the final onboarding agreement before granting access to the Dashboard.

## 2. Course Management & Payments

**Browsing & Enrolling (Student)**
* Students can navigate to the **Courses** tab to view the catalog in a grid layout.
* Each course card displays the thumbnail, instructor, syllabus, batch timings, original price, and any discounted pricing.
* Students tap **Apply** to request enrollment in a specific course.

**Invoicing & Payment Status (Admin & Student)**
* **Admin:** When a student enrolls, the administration generates an invoice for the course fee.
* **Student:** On their Dashboard, students can view their current active courses, along with a progress bar indicating course duration and their **Payment Status** (e.g., Pending, Partial, Paid) based on the generated invoice.

## 3. Communication & Support

**Enquiries System**
* **Student (Mobile):** If a student has a question or faces an issue, they can navigate to the **Enquiries** section to submit a support ticket.
* **Admin (Web Portal):** The admin logs into the Web Admin Dashboard, views all incoming enquiries in a tabular format, responds to the student, and clicks **Mark as Resolved**.

**Broadcast Notifications**
* **Admin (Mobile App):** Admins can quickly broadcast announcements to all students (e.g., "New Batch Starting" or "Fee Payment Reminders"). They enter a title and message, and tap **Send**.
* **Student (Mobile):** Students receive these notifications instantly. Unread messages are highlighted in their Notifications tab until marked as read.

**Frequently Asked Questions (FAQs)**
* Students can browse the **FAQ** tab to find quick answers to common platform questions. These FAQs are centrally managed and updated by the administration team.

## 4. Course Completion & Certification

**Marking Completion (Web Admin)**
* When a student finishes their curriculum, the Administrator logs into the **Web Admin Dashboard** and navigates to the **Registrations** tab.
* The Admin clicks the **"Mark as Completed"** button next to the student's name.

**Issuing Certificates (Web Admin)**
* Immediately after marking a course as completed, a **"📥 Download"** button appears.
* The Admin clicks this to generate a branded, official PDF **Certificate of Completion** (featuring Nu Tech Computer Education branding, digital signatures of the Center Head and Authorized Signatory, and a unique Verification ID). The admin can then print or email this to the student.

**Reviews & Ratings (Student)**
* Once completed, the course moves to the bottom of the student's mobile Dashboard with a trophy icon.
* The student can then open the course and leave a 1-to-5 star rating and a written **Review** of their experience, which will be visible on the course catalog for future students.

## 5. Profile & Settings

**Managing Profile (Student)**
* From the **Profile** tab, students can update their Name, Phone Number, DOB, Address, Education details, and upload a new Profile Photo.
* They can also securely change their password.

**Troubleshooting / Common Issues**
* **Account stuck on Pending:** The student must wait for the Admin to manually approve them.
* **Forgot Password:** Students can use the "Forgot Password" link on the login screen to receive a reset code via email.
* **Session Expiry:** For security, inactive sessions will automatically log out. Data is perfectly safe; the user simply needs to log back in.
