using System;
using System.ComponentModel.DataAnnotations;

namespace StudentOnboard.Api.Models
{
    public class StudentProfile
    {
        public int Id { get; set; }

        [Required, MaxLength(100)]
        public string FullName { get; set; }

        [Required, EmailAddress]
        public string Email { get; set; }

        [Required]
        public DateTime DateOfBirth { get; set; }

        [Required, MaxLength(300)]
        public string Address { get; set; }

        [Required, MaxLength(200)]
        public string EducationBackground { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }
}
